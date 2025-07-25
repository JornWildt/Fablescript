using System.Xml.Serialization;

namespace Fablescript.Core.Fablescript
{
  public class LocationFactDefinition
  {
    [XmlElement("Text")]
    public string Text { get; set; } = null!;
  }
}
