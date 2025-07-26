using System.Threading.Tasks;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Core.Engine;
using Fablescript.Utility.Services.CommandQuery;
using Fablescript.Utility.Services.Contract.CommandQuery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fablescript.Web.Client.Controllers
{
  public class FableController : Controller
  {
    #region Dependencies

    readonly ICommandProcessor CommandProcessor;

    #endregion


    public FableController(
      ICommandProcessor commandProcessor)
    {
      CommandProcessor = commandProcessor;
    }


    [Route("/app/fable/{id}")]
    public async Task<IActionResult> Index(string id)
    {
      var fableId = new FableId(id);
      await EnsureGameStarted(fableId);

      var playerId = CurrentPlayerId(fableId)!;

      var describeCmd = new DescribeSceneCommand(playerId, new CommandOutput<string>());
      await CommandProcessor.InvokeCommandAsync(describeCmd);

      var text = describeCmd.Answer.Value!;

      return this.Content(text);
    }

    
    private async Task EnsureGameStarted(FableId fableId)
    {
      var playerId = CurrentPlayerId(fableId);
      if (playerId == null)
      {
        var startCmd = new StartFableCommand(
          fableId,
          new CommandOutput<PlayerId>());
        await CommandProcessor.InvokeCommandAsync(startCmd);

        playerId = startCmd.CreatedPlayerId.Value!;
        SetCurrentPlayerId(fableId, playerId);
      }
    }


    private PlayerId? CurrentPlayerId(FableId fableId)
    {
      var id = HttpContext.Session.GetString(CurrentPlayerId_SessionKey(fableId));
      return id == null ? null : new PlayerId(id);
    }
    
    private void SetCurrentPlayerId(FableId fableId, PlayerId playerId) => HttpContext.Session.SetString(CurrentPlayerId_SessionKey(fableId), playerId.Value);


    private string CurrentPlayerId_SessionKey(FableId fableId) => fableId + "_PlayerId";
  }
}
