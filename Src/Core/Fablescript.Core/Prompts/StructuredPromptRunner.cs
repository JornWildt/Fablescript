using Fablescript.Core.LLM;
using Fablescript.Core.Templating;
using Microsoft.Extensions.Logging;

namespace Fablescript.Core.Prompts
{
  internal class StructuredPromptRunner : IStructuredPromptRunner
  {
    #region Dependencies

    protected readonly IPromptDefinitionProvider PromptDefinitionProvider;
    protected readonly ITemplateEngine TemplateEngine;
    protected readonly ILLMStructuredGenerator Generator;
    protected readonly ILogger Logger;

    #endregion


    public StructuredPromptRunner(
      IPromptDefinitionProvider promptDefinitionProvider,
      ITemplateEngine templateEngine,
      ILLMStructuredGenerator generator,
      ILogger<StructuredPromptRunner> logger)
    {
      PromptDefinitionProvider = promptDefinitionProvider;
      TemplateEngine = templateEngine;
      Generator = generator;
      Logger = logger;
    }


    async Task<T> IStructuredPromptRunner.RunPromptAsync<T>(string promptId, object args)
    {
      var promptDefinition = PromptDefinitionProvider.LoadPrompt(promptId, null);
      var prompt = TemplateEngine.Render(promptDefinition.Template, "Prompt", "args", args);
      var result = await Generator.GenerateStructuredOutputAsync(promptId, promptDefinition.Model, promptDefinition.Parameters, "", prompt, null, new T());

      return result;
    }
  }
}
