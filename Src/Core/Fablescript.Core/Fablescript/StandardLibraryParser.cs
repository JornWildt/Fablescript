using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Fablescript.Utility.Base;
using Fablescript.Utility.Base.Exceptions;
using Microsoft.Extensions.Logging;

namespace Fablescript.Core.Fablescript
{
  internal class StandardLibraryParser : FileWatchParser<FableDefinition, FableDefinitionSet>
  {
    public StandardLibraryParser(
      FileWatchParserConfiguration configuration,
      ILogger<StandardLibraryParser> logger)
      : base(configuration, logger)
    {
    }


    protected override FableDefinitionSet Parse()
    {
      Logger.LogDebug("Parse standard library from '{SourceDir}'.", Configuration.SourceDir);

      var fables = new FableDefinitionSet();

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
                  AddFableToFableSet(fable, fables);
              }
            }
          }

          foreach (var fable in fables.Values)
          {
            fable.Initialize(out var errors);
            if (errors.Count > 0)
            {
              foreach (var error in errors)
                Logger.LogError(error);
              throw new ParserException($"Found {errors.Count} errors in fable '{fable.Title}'.");
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

      return fables;
    }


    private void AddFableToFableSet(FableDefinition fable, FableDefinitionSet fables)
    {
    }
  }
}
