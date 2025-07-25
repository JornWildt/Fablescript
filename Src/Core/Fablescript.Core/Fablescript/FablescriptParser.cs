using System.Xml;
using System.Xml.Serialization;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Core.GameConfiguration;
using Fablescript.Utility.Base;
using Fablescript.Utility.Base.Exceptions;
using Microsoft.Extensions.Logging;

namespace Fablescript.Core.Fablescript
{
  public class FableDefinitionSet : Dictionary<string, FableDefinition> { }


  internal class FablescriptParser : FileWatchParser<FableDefinition, FableDefinitionSet>, IFablescriptParser
  {
    public FablescriptParser(
      FileWatchParserConfiguration configuration,
      ILogger<FablescriptParser> logger)
      : base(configuration, logger)
    {
    }


    // FIXME: Unused?
    FableDefinitionSet IFablescriptParser.GetResult()
    {
      return GetCache();
    }


    Task<LocationDefinition?> IFablescriptParser.TryGetAsync(FableId fableId, LocationId locationId)
    {
      var fable = GetCache()[fableId.Value];
      return Task.FromResult(fable.TryGetLocation(locationId.Value));
    }


    protected override FableDefinitionSet Parse()
    {
      Logger.LogDebug("Parse fable definitions from '{SourceDir}'.", Configuration.SourceDir);

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
              Logger.LogDebug("Parse fable from '{File}'.", filename);

              var filePathElements = filename.Split(['/', '\\']);
              var fableName = filePathElements[filePathElements.Length - 2];

              using (XmlReader reader = XmlReader.Create(filename, settings))
              {
                var fable = (FableDefinition?)serializer.Deserialize(reader);
                if (fable != null)
                  AddFableToFableSet(fableName, fable, fables);
              }
            }
          }

          foreach (var fable in fables.Values)
          {
            fable.Initialize();
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


    private void AddFableToFableSet(string fableName, FableDefinition fable, FableDefinitionSet fables)
    {
      if (!fables.TryGetValue(fableName, out var fableDefinition))
      {
        fableDefinition = new FableDefinition();
        fables[fableName] = fable;
      }

      if (!string.IsNullOrEmpty(fable.Title))
        fableDefinition.Title = fable.Title;
      if (!string.IsNullOrEmpty(fable.Description))
        fableDefinition.Description = fable.Description;
      if (!string.IsNullOrEmpty(fable.InitialLocation))
        fableDefinition.InitialLocation = fable.InitialLocation;

      foreach (var location in fable.Locations)
        fableDefinition.Locations.Add(location);
    }
  }
}
