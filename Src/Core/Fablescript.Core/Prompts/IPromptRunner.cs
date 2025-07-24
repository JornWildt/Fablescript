namespace Fablescript.Core.Prompts
{
  internal interface IPromptRunner
  {
    Task<string> RunPromptAsync(string promptId, object args);
  }
}
