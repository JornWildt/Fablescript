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


    protected string DescribeScene(
      GameState game)
    {
      var result = game.InvokeFunction("describe_scene", []) as string;
      return result ?? "";
    }

#if false
    protected async Task<string> DescribeScene(
      GameState game,
      LuaTable locationSrc)
    {
      dynamic location = new LuaObject(locationSrc);

      var player = (LuaTable)game.Player.Source;

      var objectsHere = LuaConverter.ConvertLuaTableToEnumerable((LuaTable)location.objects_here)
        .Where(o => !player.Equals(o.Source))
        .ToArray();
      var facts = LuaConverter.ConvertLuaTableToEnumerable((LuaTable)location.facts).ToArray();
      var exits = LuaConverter.ConvertLuaTableToEnumerable((LuaTable)location.exits).ToArray();

      if (DeveloperConfig?.SkipUseOfAI ?? false)
      {
        var factText = facts.Aggregate("", (a, b) => a + "\n- " + b.text);
        var exitText = exits.Aggregate("", (a, b) => a + "\n- " + b.name + ": " + b.description);
        var objects = objectsHere.Aggregate("", (a, b) => a + "\n- " + (string)b.title + ": " + (string)b.description);
        return $"### {location.title}\n{location.introduction}\n\nFacts:\n{factText}\n\nExits:\n{exitText}\n\nObjects:\n{objects}";
      }
      else
      {
        var args = new
        {
          Title = (string)location.title,
          Introduction = (string)location.introduction,
          Facts = facts.Select(f => f.text).ToArray() ?? [],
          HasFacts = facts.Length > 0,
          Exits = exits.Select(x => new { Title = x.title, Description = x.description }).ToArray(),
          HasExits = exits.Length > 0,
          Objects = objectsHere.Select(o => new { Name = (string)o.name, Title = (string)o.title, Description = (string?)o.description }).ToArray(),
          HasObjects = objectsHere.Length > 0
        };
        var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
        return response;
      }
    }
#endif


    protected async Task<string> RunPromptAsync(string promptName, LuaTable luaArgs)
    {
      var args = LuaConverter.ConvertLuaTableToDictionaryOrList(luaArgs);
      var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
      return response;
    }
  }
}
