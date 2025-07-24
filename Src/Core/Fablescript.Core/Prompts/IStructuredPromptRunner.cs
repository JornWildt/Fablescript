namespace Fablescript.Core.Prompts
{
  internal interface IStructuredPromptRunner
  {
    Task<T> RunPromptAsync<T>(string promptId, object args) where T : new();
  }
}
