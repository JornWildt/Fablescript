using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Fablescript;
using Fablescript.Core.Prompts;
using Fablescript.Utility.Services.CommandQuery;
using NLua;
using System.Dynamic;

namespace Fablescript.Core.Engine
{
  internal class FableEngine :
    ICommandHandler<DescribeSceneCommand>,
    ICommandHandler<ApplyUserInputCommand>,
    ICommandHandler<StartGameCommand>
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


    public FableEngine(
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


    async Task ICommandHandler<StartGameCommand>.InvokeAsync(StartGameCommand cmd)
    {
      var gameId = new GameId(Guid.NewGuid());
      var fable = await FablescriptParser.GetFableAsync(cmd.FableId);

      var gameState = new GameState(
        gameId,
        cmd.FableId);

      gameState.LoadScript(Path.Combine(FablescriptConfig.CoreScripts, "Utilities.lua"));
      gameState.LoadScript(Path.Combine(FablescriptConfig.CoreScripts, "Object.lua"));
      gameState.LoadScript(Path.Combine(FablescriptConfig.CoreScripts, "Core.lua"));
      gameState.Initialize();

      var locationName2ObjectMapping = new Dictionary<string, LuaObject>();

      foreach (var locDef in fable.Locations)
      {
        dynamic loc = gameState.CreateBaseObject();
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
        dynamic obj = gameState.CreateBaseObject();
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


    async Task ICommandHandler<DescribeSceneCommand>.InvokeAsync(DescribeSceneCommand cmd)
    {
      var game = await GameStateRepository.GetAsync(cmd.GameId);
      var location = (LuaTable)game.Player.location;
      var sceneDescription = await DescribeScene(game, location);
      cmd.Answer.Value = sceneDescription;
    }


    async Task ICommandHandler<ApplyUserInputCommand>.InvokeAsync(ApplyUserInputCommand cmd)
    {
      var game = await GameStateRepository.GetAsync(cmd.GameId);
      var locationSrc = (LuaTable)game.Player.location;

      dynamic location = new LuaObject(locationSrc);

      var player = (LuaTable)game.Player.Source;

      var objectsHere = LuaConverter.ConvertLuaTableToEnumerable((LuaTable)location.objects_here)
        .Where(o => !player.Equals(o.Source))
        .ToArray();
      var facts = LuaConverter.ConvertLuaTableToEnumerable((LuaTable)location.facts).ToArray();
      var exits = LuaConverter.ConvertLuaTableToEnumerable((LuaTable)location.exits).ToArray();

      var std = await StandardLibraryParser.GetStandardLibrary();
      var commands = std.Commands;

      // TODO: Command list generation for prompt can be done once and for all (for globally available commands ... without conditions)

      // FIXME: extract list of parameters such as "{object}" or "{exit}" and use it to generate the JSON intent template
      // - Afterwards, use it to match parameters on the commands

      var args = new
      {
        Location = (string)location.title,
        Facts = facts.Select(f => f.text).ToArray() ?? [],
        HasFacts = facts.Length > 0,
        Exits = exits.Select(x => new { Name = x.name, Description = x.description }).ToArray(),
        HasExits = exits.Length > 0,
        ObjectsHere = objectsHere.Select(o => new { Name = (string)o.name, Title = (string)o.title, Description = (string?)o.description }).ToArray(),
        HasObjectsHere = objectsHere.Length > 0,
        Commands = commands.Values.Select(c => new { Name = c.Name, Intention = c.Intention, Usage = c.Usage }).ToArray(),
        PlayerInput = cmd.PlayerInput
      };
      var response = await StructuredPromptRunner.RunPromptAsync<Dictionary<string,string>>("DecodeUserIntent", args);

      var decoded = response.Select(i => i.Key +  "=" + i.Value).Aggregate("", (a,b) => a + ", " + b);
      cmd.Answer.Value = "Intent: " + decoded;

      if (response.TryGetValue("intent", out string? intent))
      {
        if (intent != "other")
        {
        }
      }

      var idleResponse = await PromptRunner.RunPromptAsync("IdleUserInputResponse", args);
      cmd.Answer.Value += idleResponse;

#if false
      if (response.intent == "move" && response.move_exit_name != null)
      {
        var exit = exits.FirstOrDefault(x => x.Name == response.move_exit_name);
        if (exit != null)
        {
          var newLocationId = exit.TargetLocationId;
          LuaTable newLocation = game.GetObject(newLocationId);
          if (newLocation != null)
          {
            game.Player.LocationId = newLocationId;

            var sceneDescription = await DescribeScene(game, newLocation);
            cmd.Answer.Value = sceneDescription;
            return;
          }
        }
      }

#endif
    }


    private async Task<string> DescribeScene(
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
        return $"### {location.Title}\n{location.Introduction}\n\nFacts:\n{factText}\n\nExits:\n{exitText}\n\nObjects:\n{objects}";
      }
      else
      {
        var args = new
        {
          Title = (string)location.Title,
          Introduction = (string)location.Introduction,
          Facts = facts.Select(f => f.text).ToArray() ?? [],
          HasFacts = facts.Length > 0,
          Exits = exits.Select(x => new { Name = x.name, Description = x.description }).ToArray(),
          HasExits = exits.Length > 0,
          Objects = objectsHere.Select(o => new { Name = (string)o.name, Title = (string)o.title, Description = (string?)o.description }).ToArray(),
          HasObjects = objectsHere.Length > 0
        };
        var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
        return response;
      }
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


    public class PlayerIntent
    {
      public string? intent { get; set; }
      public string? @object { get; set; }
      public string? exit { get; set; }
    }
  }
}
