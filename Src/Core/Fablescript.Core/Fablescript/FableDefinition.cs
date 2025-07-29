using System.Xml.Serialization;
using Fablescript.Core.Engine;

namespace Fablescript.Core.Fablescript
{
  [XmlRoot("Fable", Namespace = "https://fablescript.org/schema/core")]
  public class FableDefinition
  {
    [XmlElement("Title")]
    public string Title { get; set; } = null!;


    [XmlElement("Description")]
    public string Description { get; set; } = null!;


    [XmlElement("InitialLocation")]
    public string InitialLocation { get; set; } = null!;


    [XmlArray("Locations")]
    [XmlArrayItem("Location")]
    public List<LocationDefinition> Locations { get; set; } = new List<LocationDefinition>();


    [XmlArray("Objects")]
    [XmlArrayItem("Object")]
    public List<ObjectDefinition> Objects { get; set; } = new List<ObjectDefinition>();



    protected IDictionary<string,  LocationDefinition> LocationLookup { set; get; } = new Dictionary<string, LocationDefinition>();


    public void Initialize(out List<string> errors)
    {
      errors = new List<string>();

      foreach (var location in Locations)
      {
        if (LocationLookup.ContainsKey(location.Name))
          errors.Add($"Duplication location name '{location.Name}'.");
        else
          LocationLookup[location.Name] = location;
      }

      if (!LocationLookup.ContainsKey(InitialLocation))
        errors.Add($"Initial player location '{InitialLocation}' does not match any known location.");

      foreach (var obj in Objects)
      {
        if (obj.Location != null && !LocationLookup.ContainsKey(obj.Location))
          errors.Add($"Object '{obj.Name}' location '{obj.Location}' does not match any known location.");
      }
    }


    internal LocationDefinition? TryGetLocation(string locationName)
    {
      if (LocationLookup.TryGetValue(locationName, out var location))
        return location;
      else
        return null;
    }
  }
}
