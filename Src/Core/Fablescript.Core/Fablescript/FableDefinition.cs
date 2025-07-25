using System.Xml.Serialization;

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



    protected IDictionary<string,  LocationDefinition> LocationLookup { set; get; } = new Dictionary<string, LocationDefinition>();


    public void Initialize()
    {
      foreach (var location in Locations)
      {
        LocationLookup[location.Name] = location;
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
