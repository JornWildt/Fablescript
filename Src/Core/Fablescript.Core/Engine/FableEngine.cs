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
      var location = await LocationProvider.GetAsync(player.Location);
      var args = new 
      { 
        LocationName = location.LocationName,
        Introduction = location.Introduction,
        Facts = location.Facts,
        Exits = location.Exits.Select(x => new { Id = x.Id, Name = x.Name, Description = x.Description }).ToArray()
      };
      var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
      cmd.Answer.Value = response;
    }


    Task ICommandHandler<ApplyUserInputCommand>.InvokeAsync(ApplyUserInputCommand cmd)
    {
      cmd.Answer.Value = "You said: " + cmd.UserInput;
      return Task.CompletedTask;
    }
  }
}
