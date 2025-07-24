using Fablescript.Utility.Base.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Fablescript.Utility.Services.CommandQuery
{
  public abstract class CommandProcessor<TContext> : BaseProcessor<TContext>, 
    ICommandProcessor,
    ICommandProcessor<TContext>
    where TContext : IUnitOfWorkContext
  {
    #region Dependencies
    #endregion


    public CommandProcessor(
      //IAuditService auditService,
      //IIdentityProvider identityProvider,
      //ICommandValidator validator,
      ActivitySource activitySource,
      IServiceProvider serviceProvider,
      ILogger<CommandProcessor<TContext>> logger)
      : base(activitySource, serviceProvider, logger)
    {
    }


    async Task ICommandProcessor.InvokeCommandAsync<TCmd>(TCmd cmd)
    {
      await InvokeAsync(async services =>
        {
          var handler = services.GetRequiredService<ICommandHandler<TCmd>>();
          await handler.InvokeAsync(cmd);
          return true;
        },
        cmd);
    }
  }
}
