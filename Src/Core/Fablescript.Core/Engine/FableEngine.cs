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
    private readonly DeveloperConfiguration DeveloperConfig;

    #endregion


    public FableEngine(
      IFablescriptParser fablescriptParser,
      IGameStateRepository gameStateRepository,
      IPromptRunner promptRunner,
      IStructuredPromptRunner structuredPromptRunner,
      DeveloperConfiguration developerConfig)
    {
      FablescriptParser = fablescriptParser;
      GameStateRepository = gameStateRepository;
      PromptRunner = promptRunner;
      StructuredPromptRunner = structuredPromptRunner;
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

      gameState.Initialize();

      foreach (var objDef in fable.Objects)
      {
        dynamic obj = new ExpandoObject();
        obj.Id = new ObjectId(Guid.NewGuid());
        obj.Name = objDef.Name;
        obj.Title = objDef.Title;
        obj.Description = objDef.Description;
        if (objDef.Location != null)
          obj.Location = locationIdMapping[objDef.Location];

        gameState.AddObject(obj.Id, obj);
      }

      foreach (var locDef in fable.Locations)
      {
        dynamic loc = new ExpandoObject();
        loc.Id = locationIdMapping[locDef.Name];
        loc.Name = locDef.Name;
        loc.Title = locDef.Title;
        loc.Introduction = locDef.Introduction;

        //loc.Facts = locDef.Facts.Select(Fact2FableObject).ToList();
        //loc.Exits = locDef.Exits.Select(x => Exit2FableObject(x, locationIdMapping[x.TargetLocationName])).ToList();

        gameState.AddObject(loc.Id, loc);
      }

      await GameStateRepository.AddAsync(gameState);

      cmd.CreatedGameId.Value = gameId;
    }


    async Task ICommandHandler<DescribeSceneCommand>.InvokeAsync(DescribeSceneCommand cmd)
    {
      var game = await GameStateRepository.GetAsync(cmd.GameId);
      LuaTable location = game.GetObject(game.Player.LocationId);
      var sceneDescription = await DescribeScene(game, new LuaObject(location));
      cmd.Answer.Value = sceneDescription;
    }


    async Task ICommandHandler<ApplyUserInputCommand>.InvokeAsync(ApplyUserInputCommand cmd)
    {
      var game = await GameStateRepository.GetAsync(cmd.GameId);
      dynamic location = new LuaObject(game.GetObject(game.Player.LocationId));

      dynamic[] objectsHere = game.GetAllObjects()
        .Where(o => (ObjectId)o.Location == (ObjectId)location.Id)
        .ToArray();

      var facts = (List<Fact>)location.Facts ?? new List<Fact>();
      var exits = (List<Exit>)location.Exits ?? new List<Exit>();

      var args = new
      {
        Title = (string)location.Title,
        Introduction = (string)location.Introduction,
        HasFacts = facts.Count > 0,
        Exits = exits.Select(x => new { Name = x.Name, Title = x.Title, Description = x.Description }).ToArray(),
        HasExits = exits.Count > 0,
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
    }


    private async Task<string> DescribeScene(
      GameState game,
      dynamic location)
    {
      dynamic[] objectsHere = game.GetAllObjects()
        .Where(o => (ObjectId)o.Location == (ObjectId)location.Id)
        .ToArray();

      var facts = (List<Fact>)location.Facts ?? new List<Fact>();
      var exits = (List<Exit>)location.Exits ?? new List<Exit>();

      if (!DeveloperConfig.SkipUseOfAI)
      {
        var args = new
        {
          Title = (string)location.Title,
          Introduction = (string)location.Introduction,
          Facts = facts.Select(f => f.Text).ToArray(),
          HasFacts = facts.Count > 0,
          Exits = exits.Select(x => new { Name = x.Name, Description = x.Description }).ToArray(),
          HasExits = exits.Count > 0,
          Objects = objectsHere.Select(o => new { Name = (string)o.Name, Title = (string)o.Title, Description = (string?)o.Description }).ToArray(),
          HasObjects = objectsHere.Length > 0
        };
        var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
        return response;
      }
      else
      {
        var factText = facts.Aggregate("", (a, b) => a + "\n- " + b.Text);
        var exitText = exits.Aggregate("", (a, b) => a + "\n- " + b.Name + ": " + b.Description);
        var objects = objectsHere.Aggregate("", (a, b) => a + "\n- " + (string)b.Title + ": " + (string)b.Description);
        return $"### {location.Title}\n{location.Introduction}\n\nFacts:\n{factText}\n\nExits:\n{exitText}\n\nObjects:\n{objects}";
      }
    }


    private Fact Fact2FableObject(LocationFactDefinition fact)
    {
      return new Fact(fact.Text);
    }


    private Exit Exit2FableObject(
      LocationExitDefinition x,
      ObjectId targetId)
    {
      return new Exit(
        x.Name,
        x.Title,
        x.Description,
        targetId);
    }


    public class PlayerIntent
    {
      public string? intent { get; set; }
      public string? move_exit_name { get; set; }
    }
  }


  public class Fact
  {
    public string Text { get; set; }
    
    public Fact(string text)
    {
      Text = text;
    }
  }

  public class Exit
  {
    public string Name { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public ObjectId TargetLocationId { get; set; }
    
    public Exit(string name, string title, string? description, ObjectId targetLocationId)
    {
      Name = name;
      Title = title;
      Description = description;
      TargetLocationId = targetLocationId;
    }
  }
}
