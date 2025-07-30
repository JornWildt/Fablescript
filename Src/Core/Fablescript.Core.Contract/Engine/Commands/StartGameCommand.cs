using Fablescript.Core.Contract.Fablescript;
using Fablescript.Utility.Services.Contract.CommandQuery;

namespace Fablescript.Core.Contract.Engine.Commands
{
  public record StartGameCommand(
    FableId FableId,
    CommandOutput<GameId> CreatedGameId) : ICommand
  {
  }
}
