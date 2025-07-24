using Fablescript.Utility.Services.Contract.CommandQuery;

namespace Fablescript.Core.Contract.Engine.Commands
{
  public record DescribeSceneCommand(
    PlayerId PlayerId,
    CommandOutput<string> Answer) : ICommand
  {
  }
}
