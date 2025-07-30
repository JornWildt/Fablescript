using System.Linq;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Fablescript;
using Fablescript.Core.Prompts;
using Fablescript.Utility.Services.CommandQuery;

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

      var objectNameMapping = new Dictionary<string, FableObject>();
      var locationNameMapping = new Dictionary<string, FableObject>();

      var locationIdMapping = new Dictionary<string, ObjectId>();
      foreach (var locDef in fable.Locations)
        locationIdMapping[locDef.Name] = new ObjectId(Guid.NewGuid());

      foreach (var objDef in fable.Objects)
      {
        dynamic obj = new FableObject(new ObjectId(Guid.NewGuid()), gameId);
        obj.Name = objDef.Name;
        obj.Title = objDef.Title;
        obj.Description = objDef.Description;
        if (objDef.Location != null)
          obj.Location = locationIdMapping[objDef.Location];

        objectNameMapping[objDef.Name] = obj;
      }

      foreach (var locDef in fable.Locations)
      {
        dynamic loc = new FableObject(locationIdMapping[locDef.Name], gameId);
        loc.Name = locDef.Name;
        loc.Title = locDef.Title;
        loc.Introduction = locDef.Introduction;

        loc.Facts = locDef.Facts.Select(Fact2FableObject).ToList();
        loc.Exits = locDef.Exits.Select(x => Exit2FableObject(x, locationIdMapping[x.TargetLocationName])).ToList();

        objectNameMapping[locDef.Name] = loc;
        locationNameMapping[locDef.Name] = loc;
      }

      var initialLocationId = locationNameMapping[fable.InitialLocation].Id;

      var player = new Player(
        initialLocationId);

      var gameState = new GameState(
        gameId,
        cmd.FableId,
        player,
        objectNameMapping.Values);

      await GameStateRepository.AddAsync(gameState);

      cmd.CreatedGameId.Value = gameId;
    }


    async Task ICommandHandler<DescribeSceneCommand>.InvokeAsync(DescribeSceneCommand cmd)
    {
      var game = await GameStateRepository.GetAsync(cmd.GameId);
      dynamic location = await game.GetObjectAsync(game.Player.LocationId);
      var sceneDescription = await DescribeScene(game, location);
      cmd.Answer.Value = sceneDescription;
    }


    async Task ICommandHandler<ApplyUserInputCommand>.InvokeAsync(ApplyUserInputCommand cmd)
    {
      var game = await GameStateRepository.GetAsync(cmd.GameId);
      dynamic location = await game.GetObjectAsync(game.Player.LocationId);

      var args = new
      {
        Title = (string)location.Title,
        Introduction = (string)location.Introduction,
        Facts = Array.Empty<string>(), //location.Facts,
        Exits = Array.Empty<object>(), //.Select(x => new { Id = x.Id, Name = x.Name, Description = x.Description }).ToArray(),
        PlayerInput = cmd.PlayerInput
      };
      var response = await StructuredPromptRunner.RunPromptAsync<PlayerIntent>("DecodeUserIntent", args);

      //if (response.intent == "move" && response.move_exit_id != null)
      //{
      //  var exit = location.Exits.FirstOrDefault(x => x.Id == response.move_exit_id);
      //  if (exit != null)
      //  {
      //    var newLocation = await LocationProvider.TryGetAsync(player.FableId, exit.TargetLocationId);
      //    if (newLocation != null)
      //    {
      //      player.LocationId = newLocation.Id;
            
      //      var sceneDescription = await DescribeScene(player.Id, newLocation);
      //      cmd.Answer.Value = sceneDescription;
      //      return;
      //    }
      //  }
      //}

      var idleResponse = await PromptRunner.RunPromptAsync("IdleUserInputResponse", args);
      cmd.Answer.Value += idleResponse;
    }


    private async Task<string> DescribeScene(
      GameState game,
      dynamic location)
    {
      dynamic[] objectsHere = (await game.GetAllObjectsAsync())
        .Where(o => (ObjectId)o.Location == (ObjectId)location.Id)
        .ToArray();

      if (!DeveloperConfig.SkipUseOfAI)
      {
        var args = new
        {
          Title = (string)location.Title,
          Introduction = (string)location.Introduction,
          Facts = (List<Fact>)location.Facts,
          Exits = Array.Empty<object>(), //.Select(x => new { Id = x.Id, Name = x.Name, Description = x.Description }).ToArray(),
          Objects = objectsHere.Select(o => new { Name = (string)o.Name, Title = (string)o.Title, Description = (string?)o.Description }).ToArray()
        };
        var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
        return response;
      }
      else
      {
        var facts = ((List<Fact>)location.Facts).Aggregate("", (a, b) => a + "\n- " + b.Text);
        var exits = ((List<Exit>)location.Exits).Aggregate("", (a, b) => a + "\n- " + b.Name + ": " + b.Description);
        var objects = objectsHere.Aggregate("", (a, b) => a + "\n- " + (string)b.Title + ": " + (string)b.Description);
        return $"### {location.Title}\n{location.Introduction}\n\nFacts:\n{facts}\n\nExits:\n{exits}\n\nObjects:\n{objects}";
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
      public string? move_exit_id { get; set; }
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
