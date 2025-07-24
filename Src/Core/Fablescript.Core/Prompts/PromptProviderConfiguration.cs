using System.ComponentModel.DataAnnotations;

namespace Fablescript.Core.Prompts
{
  internal class PromptProviderConfiguration
  {
    [Required]
    public string PromptDirectory { get; set; } = null!;
  }
}
