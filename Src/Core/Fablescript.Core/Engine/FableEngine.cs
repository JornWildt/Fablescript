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
    ICommandHandler<StartFableCommand>
  {
    #region Dependencies

    private readonly IFablescriptParser FablescriptParser;
    private readonly IPlayerRepository PlayerRepository;
    private readonly IObjectRepository ObjectRepository;
    private readonly ILocationProvider LocationProvider;
    private readonly IPromptRunner PromptRunner;
    private readonly IStructuredPromptRunner StructuredPromptRunner;
    private readonly DeveloperConfiguration DeveloperConfig;

    #endregion


    public FableEngine(
      IFablescriptParser fablescriptParser,
      IPlayerRepository playerRepository,
      IObjectRepository objectRepository,
      ILocationProvider locationProvider,
      IPromptRunner promptRunner,
      IStructuredPromptRunner structuredPromptRunner,
      DeveloperConfiguration developerConfig)
    {
      FablescriptParser = fablescriptParser;
      PlayerRepository = playerRepository;
      ObjectRepository = objectRepository;
      LocationProvider = locationProvider;
      PromptRunner = promptRunner;
      StructuredPromptRunner = structuredPromptRunner;
      DeveloperConfig = developerConfig;
    }


    async Task ICommandHandler<DescribeSceneCommand>.InvokeAsync(DescribeSceneCommand cmd)
    {
      var player = await PlayerRepository.GetAsync(cmd.GameId);
      var location = await LocationProvider.GetAsync(player.FableId, player.LocationId);
      var sceneDescription = await DescribeScene(player.Id, location);
      cmd.Answer.Value = sceneDescription;
    }


    async Task ICommandHandler<ApplyUserInputCommand>.InvokeAsync(ApplyUserInputCommand cmd)
    {
      var player = await PlayerRepository.GetAsync(cmd.GameId);
      var location = await LocationProvider.GetAsync(player.FableId, player.LocationId);

      var args = new
      {
        LocationName = location.Id.Value,
        Title = location.Title,
        Introduction = location.Introduction,
        Facts = location.Facts,
        Exits = location.Exits.Select(x => new { Id = x.Id, Name = x.Name, Description = x.Description }).ToArray(),
        PlayerInput = cmd.PlayerInput
      };
      var response = await StructuredPromptRunner.RunPromptAsync<PlayerIntent>("DecodeUserIntent", args);

      if (response.intent == "move" && response.move_exit_id != null)
      {
        var exit = location.Exits.FirstOrDefault(x => x.Id == response.move_exit_id);
        if (exit != null)
        {
          var newLocation = await LocationProvider.TryGetAsync(player.FableId, exit.TargetLocationId);
          if (newLocation != null)
          {
            player.LocationId = newLocation.Id;
            
            var sceneDescription = await DescribeScene(player.Id, newLocation);
            cmd.Answer.Value = sceneDescription;
            return;
          }
        }
      }

      var idleResponse = await PromptRunner.RunPromptAsync("IdleUserInputResponse", args);
      cmd.Answer.Value += idleResponse;
    }


    async Task ICommandHandler<StartFableCommand>.InvokeAsync(StartFableCommand cmd)
    {
      var gameId = new GameId(Guid.NewGuid());
      var fable = await FablescriptParser.GetFableAsync(cmd.FableId);
      var initialLocation = new LocationId(fable.InitialLocation);

      var player = new Player(
        gameId,
        cmd.FableId,
        initialLocation);

      await PlayerRepository.AddAsync(player);

      foreach (var objDef in fable.Objects)
      {
        var obj = new Object(
          new ObjectId(Guid.NewGuid()),
          gameId,
          objDef.Name,
          objDef.Title,
          objDef.Description,
          objDef.Location == null ? null : new LocationId(objDef.Location));

        await ObjectRepository.AddAsync(obj);
      }

      cmd.CreatedGameId.Value = gameId;
    }


    private async Task<string> DescribeScene(
      GameId gameId,
      Location location)
    {
      var objectsHere = (await ObjectRepository.GetAllObjectsAsync(gameId))
        .Where(o => o.Location == location.Id)
        .ToArray();

      if (!DeveloperConfig.SkipUseOfAI)
      {
        var args = new
        {
          Title = location.Title,
          LocationName = location.Id.Value,
          Introduction = location.Introduction,
          Facts = location.Facts,
          Exits = location.Exits.Select(x => new { Id = x.Id, Name = x.Name, Description = x.Description }).ToArray(),
          Objects = objectsHere.Select(o => new { Id = o.Id, Name = o.Name, Description = o.Description }).ToArray()
        };
        var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
        return response;
      }
      else
      {
        var facts = location.Facts.Aggregate("", (a, b) => a + "\n- " + b);
        var exits = location.Exits.Aggregate("", (a, b) => a + "\n- " + b.Name + ": " + b.Description);
        var objects = objectsHere.Aggregate("", (a, b) => a + "\n- " + b.Name + ": " + b.Description);
        return $"### {location.Title}\n{location.Introduction}\n\n{facts}\n\nExits:\n{exits}\n\nObjects:\n{objects}";
      }
    }


    public class PlayerIntent
    {
      public string? intent { get; set; }
      public string? move_exit_id { get; set; }
    }
  }
}
