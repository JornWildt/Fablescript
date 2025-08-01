using Fablescript.Core.Contract.Fablescript;
using Fablescript.Utility.Base;
using Fablescript.Utility.Base.Exceptions;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Serialization;

namespace Fablescript.Core.Fablescript
{
  public class StandardLibrary
  {
    public Dictionary<string, CommandDefinition> Commands { get; private init; }

    public StandardLibrary(Dictionary<string, CommandDefinition> commands)
    {
      Commands = commands;
    }
  }
  
  
  internal class StandardLibraryParser : FileWatchParser<FableDefinition, StandardLibrary>, IStandardLibraryParser
  {
    public StandardLibraryParser(
      FileWatchParserConfiguration configuration,
      ILogger<StandardLibraryParser> logger)
      : base(configuration, logger)
    {
    }


    Task<StandardLibrary> IStandardLibraryParser.GetStandardLibrary()
    {
      var lib = GetCache();
      return Task.FromResult(lib);
    }


    protected override StandardLibrary Parse()
    {
      Logger.LogDebug("Parse standard library from '{SourceDir}'.", Configuration.SourceDir);

      var commands = new Dictionary<string, CommandDefinition>();

      try
      {
        if (Configuration.SourceDir != null && Directory.Exists(Configuration.SourceDir))
        {
          XmlReaderSettings settings = CreateXmlReaderSettings("Fablescript.xsd");
          var serializer = new XmlSerializer(typeof(FableDefinition));

          foreach (var filename in Directory.EnumerateFiles(Configuration.SourceDir, "*.xml", SearchOption.AllDirectories))
          {
            if (Path.GetFileName(filename).StartsWith("_"))
            {
              Logger.LogDebug("Skipping '{File}' since the filename starts with an underscore '_'.", filename);
            }
            else
            {
              Logger.LogDebug("Parse '{File}'.", filename);

              using (XmlReader reader = XmlReader.Create(filename, settings))
              {
                var fable = (FableDefinition?)serializer.Deserialize(reader);
                if (fable != null)
                  HandeFableFile(fable, commands);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        string msg = $"Failed parsing fables from '{Configuration.SourceDir}'";
        Logger.LogError(ex, msg);
        throw new ConfigurationException(msg);
      }

      return new StandardLibrary(commands);
    }


    private void HandeFableFile(FableDefinition fable, Dictionary<string, CommandDefinition> commands)
    {
      foreach (var command in fable.Commands ?? [])
      {
        commands[command.Name] = command;
      }
    }
  }
}
