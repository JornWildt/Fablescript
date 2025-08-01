using System.ComponentModel.DataAnnotations;

namespace Fablescript.Core.Fablescript
{
  internal class FablescriptConfiguration
  {
    [Required]
    public string Fables { get; set; } = null!;
    
    [Required]
    public string SchemaDirectory { get; set; } = null!;

    [Required]
    public string CoreScripts { get; set; } = null!;

    [Required]
    public string StandardLibrary { get; set; } = null!;

    [Required]
    public string Prompts { get; set; } = null!;
  }
}
