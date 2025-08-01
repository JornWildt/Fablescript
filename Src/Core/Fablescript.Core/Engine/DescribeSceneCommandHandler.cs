using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Fablescript;
using Fablescript.Core.Prompts;
using Fablescript.Utility.Services.CommandQuery;
using NLua;

namespace Fablescript.Core.Engine
{
  internal class DescribeSceneCommandHandler :
    CommandHandlerBase,
    ICommandHandler<DescribeSceneCommand>
  {
    public DescribeSceneCommandHandler(
      IFablescriptParser fablescriptParser,
      IStandardLibraryParser standardLibraryParser,
      IGameStateRepository gameStateRepository,
      IPromptRunner promptRunner,
      IStructuredPromptRunner structuredPromptRunner,
      FablescriptConfiguration fablescriptConfig,
      DeveloperConfiguration? developerConfig = null)
      : base(
          fablescriptParser,
          standardLibraryParser,
          gameStateRepository,
          promptRunner,
          structuredPromptRunner,
          fablescriptConfig,
          developerConfig)
    {
    }


    async Task ICommandHandler<DescribeSceneCommand>.InvokeAsync(DescribeSceneCommand cmd)
    {
      var game = await GameStateRepository.GetAsync(cmd.GameId);
      var location = (LuaTable)game.Player.location;
      var sceneDescription = await DescribeScene(game, location);
      cmd.Answer.Value = sceneDescription;
    }
  }
}