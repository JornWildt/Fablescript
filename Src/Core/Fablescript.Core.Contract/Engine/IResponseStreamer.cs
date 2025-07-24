namespace Fablescript.Core.Contract.Engine
{
  public record StreamingResponseBlock(string Response, bool Done);

  public record StepDTO(string Id, string Text, string? ParentId);


  public interface IResponseStreamer
  {
    Task Step(StepDTO step);
    Task Stream(StreamingResponseBlock block);
  }


  //public record GenerateQuery(
  //  string? Query,
  //  IDictionary<string, string> Inputs,
  //  long? CaseId,
  //  long? MatterId,
  //  string? PromptId,
  //  string ExpertId,
  //  bool StructuredOutput,
  //  IReadOnlyList<ChatEntry>? History,
  //  ConversationContextEncoding? Context,
  //  IResponseStreamer? ResponseStreamer = null) : IQuery;

}
