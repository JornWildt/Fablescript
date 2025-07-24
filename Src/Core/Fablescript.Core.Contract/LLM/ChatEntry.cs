namespace Fablescript.Core.Contract.LLM
{
  public record ChatEntry(ChatRole Role, string Message);

  public enum ChatRole
  {
    System,
    User,
    Assistant,
    Tool
  }
}
