using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Fablescript;
using Fablescript.Core.Prompts;
using Fablescript.Utility.Services.CommandQuery;
using NLua;

namespace Fablescript.Core.Engine
{
  internal class DescribeSceneCommandHandler :
    ICommandHandler<DescribeSceneCommand>
  {
    #region Dependencies

    private readonly IFablescriptParser FablescriptParser;
    private readonly IStandardLibraryParser StandardLibraryParser;
    private readonly IGameStateRepository GameStateRepository;
    private readonly IPromptRunner PromptRunner;
    private readonly IStructuredPromptRunner StructuredPromptRunner;
    private readonly FablescriptConfiguration FablescriptConfig;
    private readonly DeveloperConfiguration? DeveloperConfig;

    #endregion


    public DescribeSceneCommandHandler(
      IFablescriptParser fablescriptParser,
      IStandardLibraryParser standardLibraryParser,
      IGameStateRepository gameStateRepository,
      IPromptRunner promptRunner,
      IStructuredPromptRunner structuredPromptRunner,
      FablescriptConfiguration fablescriptConfig,
      DeveloperConfiguration? developerConfig = null)
    {
      FablescriptParser = fablescriptParser;
      StandardLibraryParser = standardLibraryParser;
      GameStateRepository = gameStateRepository;
      PromptRunner = promptRunner;
      StructuredPromptRunner = structuredPromptRunner;
      FablescriptConfig = fablescriptConfig;
      DeveloperConfig = developerConfig;
    }
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