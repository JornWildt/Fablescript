using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Utility.Services.CommandQuery;

namespace Fablescript.Core.Engine
{
  internal class FableEngine :
    IFableEngine,
    ICommandHandler<DescribeSceneCommand>,
    ICommandHandler<ApplyUserInputCommand>
  {
    Task ICommandHandler<DescribeSceneCommand>.InvokeAsync(DescribeSceneCommand cmd)
    {
      cmd.Answer.Value = "A ghastly fishing town.";
      return Task.CompletedTask;
    }

    
    Task ICommandHandler<ApplyUserInputCommand>.InvokeAsync(ApplyUserInputCommand cmd)
    {
      cmd.Answer.Value = "You said: " + cmd.UserInput;
      return Task.CompletedTask;
    }
  }
}
