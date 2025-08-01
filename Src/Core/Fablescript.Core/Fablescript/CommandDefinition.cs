using System.Text.RegularExpressions;
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


    [XmlIgnore]
    public IReadOnlyList<CommandParameter> Parameters
    {
      get
      {
        if (_parameters == null)
        {
          _parameters = ParseUsage();
        }
        return _parameters;
      }
    }
    private List<CommandParameter>? _parameters;


    static Regex CommandParameterRegex = new Regex(@"\{([a-zA-Z][a-zA-Z0-9]+)(\?)?\}");

    private List<CommandParameter> ParseUsage()
    {
      var parameters = new List<CommandParameter>();
      foreach (Match match in CommandParameterRegex.Matches(Usage))
      {
        var nameGroup = match.Groups[1];
        var optionalGroup = match.Groups[2];
        parameters.Add(new CommandParameter(nameGroup.Value, optionalGroup.Success));
      }
      return parameters;
    }
  }


  public record CommandParameter(string Name, bool IsOptional);
}
