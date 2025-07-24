using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.LLM;

namespace Fablescript.Core.LLM
{
  internal interface ILLMGenerator
  {
    Task<GenerateResponse?> GenerateAsync(
      string promptId,
      string model,
      LLMParameters? parameters,
      string system,
      string prompt,
      bool outputJson,
      LLMTools? tools,
      IReadOnlyList<ChatEntry>? history,
      IResponseStreamer? responseStreamer = null);
  }

  internal record LLMParameters(double? Temperature, int? ContextSize, int? RepeatLastN, double? RepeatPenalty);

  internal record LLMTools(IReadOnlyList<LLMTool> Tools)
  {
    public string ToShortString()
    {
      return string.Join(".\n", Tools.Select(t => t.Name));
    }
  }

  internal record LLMTool(
    string Name,
    string Description,
    string Notification,
    IReadOnlyList<LLMToolParameter> Parameters,
    Func<IDictionary<string, string>, Task<ToolCallResult>> Handler,
    Func<LLMTool, IDictionary<string, string>, string> StepNotification);

  internal record LLMToolParameter(string Name, string Description);

  internal record ToolCallResult(string Response);

  internal record GenerateResponse(string Response);
}
