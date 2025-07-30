using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Utility.Services.CommandQuery;
using Fablescript.Utility.Services.Contract.CommandQuery;
using Fablescript.Web.Client.Utilities;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Fablescript.Web.Client.Controllers
{
  public class CommandRequest
  {
    public string? fableId { get; set; }
    public string? command { get; set; }
  }


  [Route("api/fable")]
  [ApiController]
  public class FableApiController : ControllerBase
  {
    #region Dependencies

    readonly ICommandProcessor CommandProcessor;
    readonly ICurrentUser CurrentUser;

    #endregion


    public FableApiController(
      ICommandProcessor commandProcessor,
      ICurrentUser currentUser)
    {
      CommandProcessor = commandProcessor;
      CurrentUser = currentUser;
    }


    [HttpPost("command")]
    public async Task<IActionResult> ExecuteCommand([FromBody] CommandRequest request)
    {
      if (string.IsNullOrWhiteSpace(request.command))
      {
        return BadRequest("command is required.");
      }
      if (string.IsNullOrWhiteSpace(request.fableId))
      {
        return BadRequest("fableId is required.");
      }

      var gameId = CurrentUser.CurrentGameId(new FableId(request.fableId));
      if (gameId != null)
      {
        if (request.command == "###DESCRIBE")
        {
          var describeCmd = new DescribeSceneCommand(gameId, new CommandOutput<string>());
          await CommandProcessor.InvokeCommandAsync(describeCmd);

          return Ok(new { message = describeCmd.Answer.Value });
        }
        else
        {
          var applyCmd = new ApplyUserInputCommand(gameId, request.command, new CommandOutput<string>());
          await CommandProcessor.InvokeCommandAsync(applyCmd);

          return Ok(new { message = applyCmd.Answer.Value });
        }
      }

      return Unauthorized();
    }
  }
}
