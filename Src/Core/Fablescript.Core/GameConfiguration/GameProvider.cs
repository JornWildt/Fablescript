namespace Fablescript.Core.GameConfiguration
{
  internal class GameProvider : IGameProvider
  {
    GameDefinition IGameProvider.Game => new GameDefinition
    {
      InitialPlayerLocation = TemporaryConstants.InitialLocationId
    };
  }
}
