using Fablescript.Core.LLM;
using Fablescript.Core.Templating;
using Microsoft.Extensions.Logging;

namespace Fablescript.Core.Prompts
{
  internal class PromptRunner : IPromptRunner
  {
    #region Dependencies

    protected readonly IPromptDefinitionProvider PromptDefinitionProvider;
    protected readonly ITemplateEngine TemplateEngine;
    protected readonly ILLMGenerator Generator;
    protected readonly ILogger Logger;

    #endregion


    public PromptRunner(
      IPromptDefinitionProvider promptDefinitionProvider,
      ITemplateEngine templateEngine,
      ILLMGenerator generator,
      ILogger<PromptRunner> logger)
    {
      PromptDefinitionProvider = promptDefinitionProvider;
      TemplateEngine = templateEngine;
      Generator = generator;
      Logger = logger;
    }


    async Task<string> IPromptRunner.RunPromptAsync(string promptId, object args)
    {
      var promptDefinition = PromptDefinitionProvider.LoadPrompt(promptId, null);
      var prompt = TemplateEngine.Render(promptDefinition.Template, "Prompt", "args", args);
      var result = await Generator.GenerateAsync(promptId, promptDefinition.Model, promptDefinition.Parameters, "", prompt, false, null, null, null);

      return result!.Response;
    }
  }
}
