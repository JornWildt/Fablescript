using System.ComponentModel.DataAnnotations;

namespace Fablescript.Core.Fablescript
{
  internal class FableScriptConfiguration
  {
    [Required]
    public string FableDirectory { get; set; } = null!;
    
    [Required]
    public string SchemaDirectory { get; set; } = null!;
  }
}
