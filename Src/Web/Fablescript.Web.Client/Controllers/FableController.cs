using System.Threading.Tasks;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Core.Engine;
using Fablescript.Utility.Services.CommandQuery;
using Fablescript.Utility.Services.Contract.CommandQuery;
using Fablescript.Web.Client.Models;
using Fablescript.Web.Client.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fablescript.Web.Client.Controllers
{
  public class FableController : Controller
  {
    #region Dependencies

    readonly ICommandProcessor CommandProcessor;
    readonly ICurrentUser CurrentUser;

    #endregion


    public FableController(
      ICommandProcessor commandProcessor,
      ICurrentUser currentUser)
    {
      CommandProcessor = commandProcessor;
      CurrentUser = currentUser;
    }


    [Route("/app/fable/{id}")]
    public async Task<IActionResult> Index(string id)
    {
      var fableId = new FableId(id);
      await EnsureGameStarted(fableId);

      var gameId = CurrentUser.CurrentGameId(fableId)!;

      var model = new FableModel
      {
        FableId = id
      };

      return View("ViewFable", model);
    }

    
    private async Task EnsureGameStarted(FableId fableId)
    {
      var gameId = CurrentUser.CurrentGameId(fableId);
      if (gameId == null)
      {
        var startCmd = new StartGameCommand(
          fableId,
          new CommandOutput<GameId>());
        await CommandProcessor.InvokeCommandAsync(startCmd);

        gameId = startCmd.CreatedGameId.Value!;
        CurrentUser.SetCurrentGameId(fableId, gameId);
      }
    }
  }
}
