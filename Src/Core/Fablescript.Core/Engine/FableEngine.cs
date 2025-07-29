using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Prompts;
using Fablescript.Utility.Services.CommandQuery;

namespace Fablescript.Core.Engine
{
  internal class FableEngine :
    IFableEngine,
    ICommandHandler<DescribeSceneCommand>,
    ICommandHandler<ApplyUserInputCommand>,
    ICommandHandler<StartFableCommand>
  {
    #region Dependencies

    private readonly IFableProvider FableProvider;
    private readonly IPlayerRepository PlayerRepository;
    private readonly ILocationProvider LocationProvider;
    private readonly IPromptRunner PromptRunner;
    private readonly IStructuredPromptRunner StructuredPromptRunner;
    private readonly DeveloperConfiguration DeveloperConfig;

    #endregion


    public FableEngine(
      IFableProvider fableProvider,
      IPlayerRepository playerRepository,
      ILocationProvider locationProvider,
      IPromptRunner promptRunner,
      IStructuredPromptRunner structuredPromptRunner,
      DeveloperConfiguration developerConfig)
    {
      FableProvider = fableProvider;
      PlayerRepository = playerRepository;
      LocationProvider = locationProvider;
      PromptRunner = promptRunner;
      StructuredPromptRunner = structuredPromptRunner;
      DeveloperConfig = developerConfig;
    }


    async Task ICommandHandler<DescribeSceneCommand>.InvokeAsync(DescribeSceneCommand cmd)
    {
      var player = await PlayerRepository.GetAsync(cmd.PlayerId);
      var location = await LocationProvider.GetAsync(player.FableId, player.LocationId);
      var sceneDescription = await DescribeScene(location);
      cmd.Answer.Value = sceneDescription;
    }


    async Task ICommandHandler<ApplyUserInputCommand>.InvokeAsync(ApplyUserInputCommand cmd)
    {
      var player = await PlayerRepository.GetAsync(cmd.PlayerId);
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
            
            var sceneDescription = await DescribeScene(newLocation);
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
      var playerId = new PlayerId("pl-" + Guid.NewGuid());
      var fable = await FableProvider.GetAsync(cmd.FableId);

      var player = new Player(
        playerId,
        cmd.FableId,
        fable.InitialLocation);

      await PlayerRepository.AddAsync(player);

      cmd.CreatedPlayerId.Value = playerId;
    }


    private async Task<string> DescribeScene(Location location)
    {
      if (!DeveloperConfig.SkipUseOfAI)
      {
        var args = new
        {
          Title = location.Title,
          LocationName = location.Id.Value,
          Introduction = location.Introduction,
          Facts = location.Facts,
          Exits = location.Exits.Select(x => new { Id = x.Id, Name = x.Name, Description = x.Description }).ToArray()
        };
        var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
        return response;
      }
      else
      {
        var facts = location.Facts.Aggregate("", (a, b) => a + "\n- " + b);
        var exits = location.Exits.Aggregate("", (a, b) => a + "\n- " + b.Name + ": " + b.Description);
        return $"### {location.Title}\n{location.Introduction}\n\n{facts}\n\nExits:\n{exits}";
      }
    }


    public class PlayerIntent
    {
      public string? intent { get; set; }
      public string? move_exit_id { get; set; }
    }
  }
}
