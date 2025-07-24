using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Fablescript.Core.LLM;
using Fablescript.Utility.Base;
using Fablescript.Utility.Base.Exceptions;
using Microsoft.Extensions.Logging;

namespace Fablescript.Core.Prompts
{
  internal class PromptDefinitionSet : Dictionary<string, PromptDefinition>
  {
  }


  internal class PromptDefinitionParser : FileWatchParser<PromptDefinition, PromptDefinitionSet>
  {
    #region Dependencies
    #endregion


    public PromptDefinitionParser(
      FileWatchParserConfiguration configuration,
      ILogger<PromptDefinitionParser> logger)
      : base(configuration, logger)
    {
    }


    public PromptDefinitionSet GetResult()
    {
      return GetCache();
    }


    protected override PromptDefinitionSet Parse()
    {
      Logger.LogDebug("Parse prompt definitions from '{SourceDir}'.", Configuration.SourceDir);

      var prompts = new PromptDefinitionSet();

      try
      {
        if (Configuration.SourceDir != null && Directory.Exists(Configuration.SourceDir))
        {
          foreach (var filename in Directory.EnumerateFiles(Configuration.SourceDir, "*.txt", SearchOption.AllDirectories))
          {
            LoadPromptFile(filename, prompts);
          }
        }
      }
      catch (Exception ex)
      {
        string msg = $"Failed parsing prompts from '{Configuration.SourceDir}'";
        Logger.LogError(ex, msg);
        throw new ConfigurationException(msg);
      }

      return prompts;
    }


    void LoadPromptFile(string filename, PromptDefinitionSet prompts)
    {
      var name = Path.GetFileName(filename);

      if (name == "README.TXT")
        return;

      if (name.StartsWith("_"))
      {
        Logger.LogDebug("Skipping '{File}' since the filename starts with an underscore '_'.", filename);
        return;
      }

      Logger.LogDebug("Parse prompt from '{File}'.", filename);
      try
      {
        var parameterRegex = new Regex("^--- ([a-zA-Z0-9_]+):(.+)$"); // FIXME: Move

        string? model = null;
        double temperature = 0.0;
        int? contentSize = null;
        StringBuilder content = new StringBuilder();

        foreach (var line in File.ReadLines(filename))
        {
          var match = parameterRegex.Match(line);
          if (match.Success)
          {
            string key = match.Groups[1].Value.Trim();
            string value = match.Groups[2].Value.Trim();
            if (key == "Model")
            {
              model = value;
            }
            else if (key == "Temperature")
            {
              if (double.TryParse(value, CultureInfo.InvariantCulture, out var t))
                temperature = t;
              else
                throw new ConfigurationException($"Could not parse temperature value '{value}' for prompt '{filename}'.");
            }
            else if (key == "ContextSize")
            {
              if (int.TryParse(value, CultureInfo.InvariantCulture, out var cs))
                contentSize = cs;
              else
                throw new ConfigurationException($"Could not parse content size value '{value}' for prompt '{filename}'.");
            }
          }
          else
          {
            content.AppendLine(line);
          }
        }

        if (model == null)
          throw new ConfigurationException($"Model not specified for prompt '{filename}'.");

        var prompt = new PromptDefinition(
          model,
          new LLMParameters(temperature, contentSize, null, null),
          content.ToString());

        prompts[name] = prompt;
      }
      catch (Exception ex)
      {
        // Log and ignore error - continue with other files.
        Logger.LogError(ex, "Failed parsing prompt from '{File}'.", filename);
      }
    }
  }
}
