using System.Xml.Serialization;

namespace Fablescript.Core.Fablescript
{
  public class ObjectDefinition
  {
    [XmlElement("Name")]
    public string Name { get; set; } = null!;

    [XmlElement("Title")]
    public string Title { get; set; } = null!;

    [XmlElement("Description")]
    public string? Description { get; set; }

    [XmlElement("Location")]
    public string? Location { get; set; }
  }
}
