using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Fablescript;
using Fablescript.Core.Prompts;
using Fablescript.Utility.Services.CommandQuery;
using NLua;

namespace Fablescript.Core.Engine
{
  internal class StartGameCommandHandler :
    CommandHandlerBase,
    ICommandHandler<StartGameCommand>
  {
    public StartGameCommandHandler(
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


    async Task ICommandHandler<StartGameCommand>.InvokeAsync(StartGameCommand cmd)
    {
      var gameId = new GameId(Guid.NewGuid());
      var fable = await FablescriptParser.GetFableAsync(cmd.FableId);

      var gameState = new GameState(
        gameId,
        cmd.FableId);

      gameState.AddFunction("Core", "say", new Action<string>(msg => { gameState.ResponseOutput.Add(msg); }));
      gameState.AddFunction("Core", "run_prompt", new Func<string,LuaTable,string>((promptName, args) => RunPromptAsync(promptName, args).GetAwaiter().GetResult()));

      gameState.LoadScript(Path.Combine(FablescriptConfig.CoreScripts, "Fun.lua"));
      gameState.LoadScript(Path.Combine(FablescriptConfig.CoreScripts, "Utilities.lua"));
      gameState.LoadScript(Path.Combine(FablescriptConfig.CoreScripts, "Object.lua"));
      gameState.LoadScript(Path.Combine(FablescriptConfig.CoreScripts, "Commands.lua"));
      gameState.LoadScript(Path.Combine(FablescriptConfig.CoreScripts, "Core.lua"));

      gameState.Initialize();

      dynamic player = gameState.CreateBaseObject(null, "Player");
      player.name = "Player";
      gameState.Player = player;

      var locationName2ObjectMapping = new Dictionary<string, LuaObject>();

      foreach (var locDef in fable.Locations)
      {
        dynamic loc = gameState.CreateBaseObject("Objects", locDef.Name);
        loc.name = locDef.Name;
        loc.title = locDef.Title;
        loc.introduction = locDef.Introduction;

        loc.facts = gameState.CreateLuaArray(locDef.Facts.Select(f => Fact2LuaTable(gameState, f)));

        locationName2ObjectMapping[locDef.Name] = loc;
      }

      foreach (var locDef in fable.Locations)
      {
        dynamic loc = locationName2ObjectMapping[locDef.Name];
        var exits = locDef.Exits.Select(x => Exit2LuaTable(gameState, x, locationName2ObjectMapping[x.TargetLocationName]));
        loc.exits = gameState.CreateLuaArray(exits);
      }

      var playerInitialLocation = locationName2ObjectMapping[fable.InitialLocation].Source;
      gameState.InvokeMethod(gameState.Player.Source, "move_to", playerInitialLocation);

      foreach (var objDef in fable.Objects)
      {
        dynamic obj = gameState.CreateBaseObject("Objects", objDef.Name);
        obj.name = objDef.Name;
        obj.title = objDef.Title;
        obj.description = objDef.Description;
        if (objDef.Location != null)
        {
          var location = locationName2ObjectMapping[objDef.Location].Source;
          gameState.InvokeMethod(obj.Source, "move_to", location);
        }
      }

      await GameStateRepository.AddAsync(gameState);

      cmd.CreatedGameId.Value = gameId;
    }


    private LuaTable Fact2LuaTable(
      GameState gameState,
      LocationFactDefinition fact)
    {
      var obj = gameState.CreateEmptyLuaTable();
      obj["text"] = fact.Text;
      return obj;
    }


    private LuaTable Exit2LuaTable(
      GameState gameState,
      LocationExitDefinition x,
      LuaObject target)
    {
      var exit = gameState.CreateEmptyLuaTable();
      exit["name"] = x.Name;
      exit["title"] = x.Title;
      exit["description"] = x.Description;
      exit["targetLocation"] = target.Source;

      return exit;
    }
  }
}
