using Fablescript.Utility.Services.Contract.CommandQuery;

namespace Fablescript.Core.Contract.Engine.Commands
{
  public record ApplyUserInputCommand(
    PlayerId PlayerId,
    string PlayerInput,
    CommandOutput<string> Answer) : ICommand
  {
  }
}
