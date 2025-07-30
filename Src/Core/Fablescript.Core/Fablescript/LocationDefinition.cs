using System.Xml.Serialization;

namespace Fablescript.Core.Fablescript
{
  public class LocationDefinition
  {
    [XmlElement("Name")]
    public string Name { get; set; } = null!;

    [XmlElement("Title")]
    public string Title { get; set; } = null!;

    [XmlElement("Introduction")]
    public string? Introduction { get; set; }

    [XmlArray("Facts")]
    [XmlArrayItem("Fact")]
    public LocationFactDefinition[] Facts { get; set; } = new LocationFactDefinition[0];

    [XmlArray("Exits")]
    [XmlArrayItem("Exit")]
    public LocationExitDefinition[] Exits { get; set; } = new LocationExitDefinition[0];
  }
}
