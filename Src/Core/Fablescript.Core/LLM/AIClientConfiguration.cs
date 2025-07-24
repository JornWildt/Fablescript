using System.ComponentModel.DataAnnotations;
using Fablescript.Utility.Base.Configuration;

namespace Fablescript.Core.LLM
{
  internal enum AIClientTypeEnum
  {
    Ollama,
    OpenAI
  }


  internal class AIClientConfiguration : IValidatableObject
  {
    [MergeableAppSetting]
    public string? Url { get; set; } = null!;

    public string ApiKey { get; set; } = "";

    [Required]
    public AIClientTypeEnum? Type { get; set; }

    // Currently only used for automatic test setup
    public string? PreferredModel { get; set; }
    public string? PreferredEmbeddingModel { get; set; }

    IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
    {
      if (Type == AIClientTypeEnum.OpenAI)
      {
        if (string.IsNullOrEmpty(ApiKey))
          yield return new ValidationResult("AI Client configuration must include an API key when using OpenAI.");
      }
    }
  }
}
