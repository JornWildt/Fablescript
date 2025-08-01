using Fablescript.Core.Fablescript;
using Fablescript.Core.Prompts;
using NLua;

namespace Fablescript.Core.Engine
{
  internal class CommandHandlerBase
  {
    #region Dependencies

    protected readonly IFablescriptParser FablescriptParser;
    protected readonly IStandardLibraryParser StandardLibraryParser;
    protected readonly IGameStateRepository GameStateRepository;
    protected readonly IPromptRunner PromptRunner;
    protected readonly IStructuredPromptRunner StructuredPromptRunner;
    protected readonly FablescriptConfiguration FablescriptConfig;
    protected readonly DeveloperConfiguration? DeveloperConfig;

    #endregion


    public CommandHandlerBase(
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
}
