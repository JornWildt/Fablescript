using Fablescript.Utility.Base.Exceptions;

namespace Fablescript.Core.Prompts
{
  internal class TextFilePromptDefinitionProvider : IPromptDefinitionProvider
  {
    #region Dependencies
    #endregion


    PromptDefinitionParser PromptParser;


    public TextFilePromptDefinitionProvider(
      PromptDefinitionParser promptParser)
    {
      PromptParser = promptParser;
    }


    PromptDefinition IPromptDefinitionProvider.LoadPrompt(string name, string? language)
    {
      var prompts = PromptParser.GetResult();

      string filename = name + (language != null ? "." + language : "") + ".txt";
      if (prompts.TryGetValue(filename, out var prompt))
      {
        return prompt;
      }
      else
        throw new ConfigurationException($"Could not find a prompt named '{name}'.");
    }
  }
}
