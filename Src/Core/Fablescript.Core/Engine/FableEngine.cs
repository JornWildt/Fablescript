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

      foreach (var objDef in fable.Objects)
      {
        dynamic obj = new FableObject(new ObjectId(Guid.NewGuid()), gameId);
        obj.Name = objDef.Name;
        obj.Title = objDef.Title;
        obj.Description = objDef.Description;

        objectNameMapping[objDef.Name] = obj;
      }

      foreach (var locDef in fable.Locations)
      {
        dynamic loc = new FableObject(new ObjectId(Guid.NewGuid()), gameId);
        loc.Name = locDef.Name;
        loc.Title = locDef.Title;
        loc.Introduction = locDef.Introduction;

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
          Facts = Array.Empty<string>(), //location.Facts,
          Exits = Array.Empty<object>(), //.Select(x => new { Id = x.Id, Name = x.Name, Description = x.Description }).ToArray(),
          Objects = objectsHere.Select(o => new { Id = o.Id, Name = o.Name, Description = o.Description }).ToArray()
        };
        var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
        return response;
      }
      else
      {
        var facts = "none"; // location.Facts.Aggregate("", (a, b) => a + "\n- " + b);
        var exits = "none"; // location.Exits.Aggregate("", (a, b) => a + "\n- " + b.Name + ": " + b.Description);
        var objects = objectsHere.Aggregate("", (a, b) => a + "\n- " + b.Name + ": " + b.Description);
        return $"### {location.Title}\n{location.Introduction}\n\nFacts:\n{facts}\n\nExits:\n{exits}\n\nObjects:\n{objects}";
      }
    }


    public class PlayerIntent
    {
      public string? intent { get; set; }
      public string? move_exit_id { get; set; }
    }
  }
}
