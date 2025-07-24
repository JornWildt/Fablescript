using Fablescript.Core.LLM;

namespace Fablescript.Core.Prompts
{
  internal record PromptDefinition(string Model, LLMParameters Parameters, string Template);
}
