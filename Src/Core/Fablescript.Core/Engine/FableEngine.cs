using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.GameConfiguration;
using Fablescript.Core.Prompts;
using Fablescript.Utility.Services.CommandQuery;

namespace Fablescript.Core.Engine
{
  internal class FableEngine :
    IFableEngine,
    ICommandHandler<DescribeSceneCommand>,
    ICommandHandler<ApplyUserInputCommand>
  {
    #region Dependencies

    private readonly IPlayerRepository PlayerRepository;
    private readonly ILocationProvider LocationProvider;
    private readonly IPromptRunner PromptRunner;
    private readonly IStructuredPromptRunner StructuredPromptRunner;

    #endregion


    public FableEngine(
      IPlayerRepository playerRepository,
      ILocationProvider locationProvider,
      IPromptRunner promptRunner,
      IStructuredPromptRunner structuredPromptRunner)
    {
      PlayerRepository = playerRepository;
      LocationProvider = locationProvider;
      PromptRunner = promptRunner;
      StructuredPromptRunner = structuredPromptRunner;
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
        LocationName = location.LocationName,
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


    private async Task<string> DescribeScene(Location location)
    {
      var args = new
      {
        LocationName = location.LocationName,
        Introduction = location.Introduction,
        Facts = location.Facts,
        Exits = location.Exits.Select(x => new { Id = x.Id, Name = x.Name, Description = x.Description }).ToArray()
      };
      var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
      return response;
    }

    public class PlayerIntent
    {
      public string? intent { get; set; }
      public string? move_exit_id { get; set; }
    }
  }
}
