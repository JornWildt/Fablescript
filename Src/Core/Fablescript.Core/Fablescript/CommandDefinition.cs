using System.Xml.Serialization;

namespace Fablescript.Core.Fablescript
{
  public class CommandDefinition
  {
    [XmlElement("Name")]
    public string Name { get; set; } = null!;

    [XmlElement("Intention")]
    public string Intention { get; set; } = null!;

    [XmlElement("Usage")]
    public string Usage { get; set; } = null!;

    [XmlElement("Invoke")]
    public string Invoke { get; set; } = null!;
  }
}
