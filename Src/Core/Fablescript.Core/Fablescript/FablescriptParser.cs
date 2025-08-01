﻿using Fablescript.Core.Contract.Fablescript;
using Fablescript.Utility.Base;
using Fablescript.Utility.Base.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Xml;
using System.Xml.Serialization;

namespace Fablescript.Core.Fablescript
{
  internal class FableDefinitionSet : ConcurrentDictionary<string, FableDefinition> { }


  internal class FablescriptParser : FileWatchParser<FableDefinition, FableDefinitionSet>, IFablescriptParser
  {
    public FablescriptParser(
      FileWatchParserConfiguration configuration,
      ILogger<FablescriptParser> logger)
      : base(configuration, logger)
    {
    }


    #region IFablescriptParser

    Task<FableDefinition> IFablescriptParser.GetFableAsync(FableId fableId)
    {
      var fable = GetCache()[fableId.Value];
      return Task.FromResult(fable);
    }

    #endregion


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
