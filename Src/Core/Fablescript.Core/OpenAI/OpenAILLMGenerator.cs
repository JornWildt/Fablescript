using System.ClientModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Web;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.LLM;
using Fablescript.Core.LLM;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using LLMChatRole = global::Fablescript.Core.Contract.LLM.ChatRole;

namespace Fablescript.Core.OpenAI
{
  internal class OpenAILLMGenerator : ILLMGenerator
  {
    #region Dependencies

    readonly AIClientConfiguration AIClientConfig;
    readonly ILogger Logger;

    #endregion


    public OpenAILLMGenerator(
      AIClientConfiguration aiClientConfig,
      ILogger<LLMLogger> logger)
    {
      AIClientConfig = aiClientConfig;
      Logger = logger;
    }


    async Task<GenerateResponse?> ILLMGenerator.GenerateAsync(
      string promptId,
      string model,
      LLMParameters? parameters,
      string system,
      string prompt,
      bool outputJson,
      LLMTools? tools,
      IReadOnlyList<ChatEntry>? history,
      IResponseStreamer? responseStreamer)
    {
      Logger.LogDebug("OpenAI generate with {Model} for: \nSystem:\n{System}\nPrompt:\n{Prompt}\nTools:\n{Tools}",
        model, system, prompt, tools != null ? tools.ToShortString() : "-none-");

      var openAITools = GetTools(tools);

      var messages =
        new SystemChatMessage[] { ChatMessage.CreateSystemMessage(system) }
        .Concat((history ?? [])
          .Select(GetMessage)
          .Append(ChatMessage.CreateUserMessage(prompt)))
        .ToList();

      var client = new ChatClient(
        model,
        new ApiKeyCredential(AIClientConfig.ApiKey),
        new OpenAIClientOptions { Endpoint = AIClientConfig.Url != null ? new Uri(AIClientConfig.Url) : null });

      var options = new ChatCompletionOptions
      {
        ResponseFormat = outputJson ? ChatResponseFormat.CreateJsonObjectFormat() : ChatResponseFormat.CreateTextFormat(),
        Temperature = (float?)parameters?.Temperature
      };

      foreach (var t in openAITools ?? [])
        options.Tools.Add(t);

      var sw = new Stopwatch();

      int loopCount = 0;
      GenerateResponse? toolResult = null;
      bool requiresAction;

      do
      {
        requiresAction = false;

        sw.Start();
        var (content, toolCalls, usage, finishReason) = await StreamCompletion(client, options, messages, responseStreamer);
        sw.Stop();

        if (finishReason != null)
        {
          switch (finishReason)
          {
            case ChatFinishReason.Stop:
              {
                // Add the assistant message to the conversation history.
                messages.Add(new AssistantChatMessage(content));

                toolResult = new GenerateResponse(content);
                break;
              }
            case ChatFinishReason.ToolCalls:
              {
                if (tools != null)
                {
                  messages.Add(new AssistantChatMessage(toolCalls));

                  foreach (var toolCall in toolCalls)
                  {
                    var tool = tools.Tools.FirstOrDefault(t => t.Name == toolCall.FunctionName);

                    if (tool != null)
                    {
                      using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

                      var arguments = new Dictionary<string, string>();

                      foreach (var p in argumentsJson.RootElement.EnumerateObject())
                      {
                        if (!string.IsNullOrEmpty(p.Name) && !string.IsNullOrEmpty(p.Value.ToString()))
                        {
                          arguments[p.Name] = p.Value.ToString(); // FIXME: Need type checking and conversion
                        }
                      }

                      if (responseStreamer != null)
                      {
                        await responseStreamer.Step(new StepDTO(Guid.NewGuid().ToString(), tool.StepNotification(tool, arguments), null));
                      }

                      Logger.LogDebug("Use tool: {Tool}({Arguments})", toolCall.FunctionName, string.Join(", ", arguments.Select(a => $"{a.Key} = {a.Value}")));
                      var toolSubResult = await tool.Handler(arguments);
                      Logger.LogDebug("Tool result: {Tool} => {Result}", toolCall.FunctionName, toolSubResult.Response);

                      messages.Add(new ToolChatMessage(toolCall.Id, toolSubResult.Response));
                      toolResult = new GenerateResponse(toolResult?.Response + " " + toolSubResult.Response);
                    }
                  }
                }
                requiresAction = true;
              }
              break;
            default:
              Logger.LogDebug("OpenAI unhandled finish reason: {Reason}", finishReason);
              break;
          }
        }
        else
          Logger.LogDebug("OpenAI completed unexpectedly with no finish reason.");
      }
      while (requiresAction && ++loopCount <= 5);

      Logger.LogDebug("Generated OpenAI chat response: {Response}", (toolResult?.Response ?? "-none-").Trim());

      return toolResult;
    }


    async Task<(string, List<ChatToolCall>, ChatTokenUsage?, ChatFinishReason?)> StreamCompletion(
      ChatClient client,
      ChatCompletionOptions options,
      List<ChatMessage> messages,
      IResponseStreamer? responseStreamer)
    {
      StringBuilder resultBuilder = new StringBuilder();
      ChatTokenUsage? usage = null;
      ChatFinishReason? finishReason = null;

      string? functionName = null;
      string functionArguments = "";
      string? toolCallId = null;
      List<ChatToolCall> toolCallList = new List<ChatToolCall>();

      AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = client.CompleteChatStreamingAsync(messages, options);
      await foreach (var completionUpdate in completionUpdates)
      {
        if (completionUpdate.ContentUpdate.Count > 0 && !string.IsNullOrEmpty(completionUpdate.ContentUpdate[0].Text))
        {
          resultBuilder.Append(completionUpdate.ContentUpdate[0].Text);
          if (responseStreamer != null)
          {
            await responseStreamer.Stream(new StreamingResponseBlock(completionUpdate.ContentUpdate[0].Text, false));
          }
        }

        foreach (var toolCallUpdate in completionUpdate.ToolCallUpdates ?? [])
        {
          if (toolCallUpdate.ToolCallId != null)
          {
            // Add collected tool call when tool call ID changes.
            if (toolCallId != null && functionName != null && functionArguments != null)
            {
              toolCallList.Add(ChatToolCall.CreateFunctionToolCall(toolCallId, functionName, new BinaryData(functionArguments)));
              functionArguments = "";
            }
            toolCallId = toolCallUpdate.ToolCallId;
          }
          if (toolCallUpdate.FunctionName != null)
          {
            functionName = toolCallUpdate.FunctionName;
          }
          if (toolCallUpdate.FunctionArgumentsUpdate != null && toolCallUpdate.FunctionArgumentsUpdate.ToMemory().Length > 0)
          {
            functionArguments += toolCallUpdate.FunctionArgumentsUpdate.ToString();
          }
        }

        if (completionUpdate.FinishReason != null && responseStreamer != null)
        {
          await responseStreamer.Stream(new StreamingResponseBlock("", true));
        }

        if (completionUpdate.Usage != null)
          usage = completionUpdate.Usage;
        if (completionUpdate.FinishReason != null)
          finishReason = completionUpdate.FinishReason;
      }

      // Add remaining collected tool call when tool call ID changes.
      if (toolCallId != null && functionName != null && functionArguments != null
        && (toolCallList.Count == 0 || toolCallList[toolCallList.Count - 1].Id != toolCallId))
      {
        toolCallList.Add(ChatToolCall.CreateFunctionToolCall(toolCallId, functionName, new BinaryData(functionArguments)));
      }

      return (resultBuilder.ToString(), toolCallList, usage, finishReason);
    }


    ChatMessage GetMessage(ChatEntry c)
    {
      if (c.Role == LLMChatRole.System)
        return ChatMessage.CreateSystemMessage(c.Message);
      else if (c.Role == LLMChatRole.User)
        return ChatMessage.CreateUserMessage(c.Message);
      else if (c.Role == LLMChatRole.Assistant)
        return ChatMessage.CreateAssistantMessage(c.Message);
      else if (c.Role == LLMChatRole.Tool)
        return ChatMessage.CreateToolMessage(c.Message);
      else
        throw new ArgumentException($"Unexpected chat entry role '{c.Role}'.");
    }


    List<ChatTool>? GetTools(LLMTools? tools)
    {
      if (tools == null)
        return null;

      return tools.Tools
        .Where(t => t != null)
        .Select(ExpertToolToOpenAI)
        .ToList();
    }


    ChatTool ExpertToolToOpenAI(LLMTool tool)
    {
      var parameters = tool.Parameters
        .Select(p => $"\"{p.Name}\": {{ \"type\": \"string\", \"description\": \"{JsonEncode(p.Description)}\" }}");

      var parametersString = string.Join(", ", parameters);

      return ChatTool.CreateFunctionTool(
        functionName: tool.Name,
        functionDescription: tool.Description,
        functionParameters: BinaryData.FromBytes(Encoding.UTF8.GetBytes("""
{
    "type": "object",
    "properties": {
""" + parametersString + """
    },
    "required": []
}
""").ToArray())
        );
    }


    private string JsonEncode(string text) => HttpUtility.JavaScriptStringEncode(text);
  }
}
