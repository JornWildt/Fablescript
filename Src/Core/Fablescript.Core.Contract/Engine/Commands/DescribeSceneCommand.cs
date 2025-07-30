using Fablescript.Utility.Services.Contract.CommandQuery;

namespace Fablescript.Core.Contract.Engine.Commands
{
  /// <summary>
  /// Describe scene for the game's player's current location
  /// </summary>
  /// <param name="GameId"></param>
  /// <param name="Answer"></param>
  public record DescribeSceneCommand(
    GameId GameId,
    CommandOutput<string> Answer) : ICommand
  {
  }
}
