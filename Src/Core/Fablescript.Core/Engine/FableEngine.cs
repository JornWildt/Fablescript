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
    private readonly IGameStateRepository GameStateRepository;
    private readonly IPromptRunner PromptRunner;
    private readonly IStructuredPromptRunner StructuredPromptRunner;
    private readonly FablescriptConfiguration FablescriptConfig;
    private readonly DeveloperConfiguration DeveloperConfig;

    #endregion


    public FableEngine(
      IFablescriptParser fablescriptParser,
      IGameStateRepository gameStateRepository,
      IPromptRunner promptRunner,
      IStructuredPromptRunner structuredPromptRunner,
      FablescriptConfiguration fablescriptConfig,
      DeveloperConfiguration developerConfig)
    {
      FablescriptParser = fablescriptParser;
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

      var locationIdMapping = new Dictionary<string, ObjectId>();
      foreach (var locDef in fable.Locations)
        locationIdMapping[locDef.Name] = new ObjectId(Guid.NewGuid());

      var initialLocationId = locationIdMapping[fable.InitialLocation];

      var player = new Player(
        initialLocationId);

      var gameState = new GameState(
        gameId,
        cmd.FableId,
        player);

      gameState.LoadScript(Path.Combine(FablescriptConfig.CoreScripts, "Utilities.lua"));
      gameState.LoadScript(Path.Combine(FablescriptConfig.CoreScripts, "Object.lua"));
      gameState.LoadScript(Path.Combine(FablescriptConfig.CoreScripts, "Core.lua"));
      gameState.Initialize();

      foreach (var objDef in fable.Objects)
      {
        dynamic obj = new ExpandoObject();
        obj.id = new ObjectId(Guid.NewGuid());
        obj.name = objDef.Name;
        obj.title = objDef.Title;
        obj.description = objDef.Description;
        if (objDef.Location != null)
          obj.location = locationIdMapping[objDef.Location];

        gameState.AddObject(obj.id, obj);
      }

      foreach (var locDef in fable.Locations)
      {
        dynamic loc = new ExpandoObject();
        loc.id = locationIdMapping[locDef.Name];
        loc.name = locDef.Name;
        loc.title = locDef.Title;
        loc.introduction = locDef.Introduction;

        loc.facts = locDef.Facts.Select(Fact2Expando).ToArray();
        loc.exits = locDef.Exits.Select(x => Exit2Expando(x, locationIdMapping[x.TargetLocationName])).ToArray();

        gameState.AddObject(loc.id, loc);
      }

      await GameStateRepository.AddAsync(gameState);

      cmd.CreatedGameId.Value = gameId;
    }


    async Task ICommandHandler<DescribeSceneCommand>.InvokeAsync(DescribeSceneCommand cmd)
    {
      var game = await GameStateRepository.GetAsync(cmd.GameId);
      var location = game.GetObject(game.Player.LocationId);
      var sceneDescription = await DescribeScene(game, location);
      cmd.Answer.Value = sceneDescription;
    }


    async Task ICommandHandler<ApplyUserInputCommand>.InvokeAsync(ApplyUserInputCommand cmd)
    {
      var game = await GameStateRepository.GetAsync(cmd.GameId);
      LuaObject location = game.GetObject(game.Player.LocationId);

      game.InvokeMethod(location.Source, "inspect");

#if false
      string locationId = location.Id;

      dynamic[] objectsHere = game.GetAllObjects()
        .Where(o => (ObjectId)o.Location == locationId)
        .ToArray();

      var facts = LuaConverter.ConvertLuaTableToArray((LuaTable)location.facts).ToArray();
      var exits = LuaConverter.ConvertLuaTableToArray((LuaTable)location.exits).ToArray();

      var args = new
      {
        Title = (string)location.Title,
        Introduction = (string)location.Introduction,
        HasFacts = facts.Length > 0,
        Exits = exits.Select(x => new { Name = x.Name, Title = x.Title, Description = x.Description }).ToArray(),
        HasExits = exits.Length > 0,
        Objects = objectsHere.Select(o => new { Name = (string)o.Name, Title = (string)o.Title, Description = (string?)o.Description }).ToArray(),
        HasObjects = objectsHere.Length > 0,
        PlayerInput = cmd.PlayerInput
      };
      var response = await StructuredPromptRunner.RunPromptAsync<PlayerIntent>("DecodeUserIntent", args);

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

      var idleResponse = await PromptRunner.RunPromptAsync("IdleUserInputResponse", args);
      cmd.Answer.Value += idleResponse;
#endif
    }


    private async Task<string> DescribeScene(
      GameState game,
      dynamic location)
    {
      dynamic[] objectsHere = game.GetAllObjects()
        .Where(o => (string)o.location == (string)location.id)
        .ToArray();

      var facts = LuaConverter.ConvertLuaTableToArray((LuaTable)location.facts).ToArray();
      var exits = LuaConverter.ConvertLuaTableToArray((LuaTable)location.exits).ToArray();

      if (!DeveloperConfig.SkipUseOfAI)
      {
        var args = new
        {
          Title = (string)location.Title,
          Introduction = (string)location.Introduction,
          Facts = facts.Select(f => f.Text).ToArray() ?? [],
          HasFacts = facts.Length > 0,
          Exits = exits.Select(x => new { Name = x.Name, Description = x.Description }).ToArray(),
          HasExits = exits.Length > 0,
          Objects = objectsHere.Select(o => new { Name = (string)o.name, Title = (string)o.title, Description = (string?)o.description }).ToArray(),
          HasObjects = objectsHere.Length > 0
        };
        var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
        return response;
      }
      else
      {
        var factText = facts.Aggregate("", (a, b) => a + "\n- " + b.text);
        var exitText = exits.Aggregate("", (a, b) => a + "\n- " + b.name + ": " + b.description);
        var objects = objectsHere.Aggregate("", (a, b) => a + "\n- " + (string)b.title + ": " + (string)b.description);
        return $"### {location.Title}\n{location.Introduction}\n\nFacts:\n{factText}\n\nExits:\n{exitText}\n\nObjects:\n{objects}";
      }
    }


    private ExpandoObject Fact2Expando(LocationFactDefinition fact)
    {
      dynamic obj = new ExpandoObject();
      obj.text = fact.Text;
      return obj;
    }


    private ExpandoObject Exit2Expando(
      LocationExitDefinition x,
      ObjectId targetId)
    {
      dynamic exit = new ExpandoObject();
      exit.name = x.Name;
      exit.title = x.Title;
      exit.description = x.Description;
      exit.targetLocationId = targetId;

      return exit;
    }


    public class PlayerIntent
    {
      public string? intent { get; set; }
      public string? move_exit_name { get; set; }
    }
  }
}
