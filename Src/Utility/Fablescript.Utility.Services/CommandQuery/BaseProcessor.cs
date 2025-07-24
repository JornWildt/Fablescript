using Fablescript.Utility.Base.Exceptions;
using Fablescript.Utility.Base.UnitOfWork;
using Fablescript.Utility.Services.Contract.CommandQuery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Fablescript.Utility.Services.CommandQuery
{
  public abstract class BaseProcessor<TContext>
    where TContext : IUnitOfWorkContext
  {
    #region Dependencies

    protected readonly ActivitySource ActivitySource;
    protected readonly IServiceProvider Services;
    protected readonly ILogger Logger;

    #endregion

    // FIXME: Implement audit, identity, validation and similar as plugins to the processor, yielding a pipeline in itself.

    public BaseProcessor(
      //IAuditService auditService,
      //IIdentityProvider identityProvider,
      //ICommandValidator validator,
      ActivitySource activitySource,
      IServiceProvider services,
      ILogger logger)
    {
      ActivitySource = activitySource;
      Services = services;
      Logger = logger;
    }


    protected async Task<TResult> InvokeAsync<TAction, TResult>(Func<IServiceProvider, Task<TResult>> actionHandler, TAction action)
      where TAction : notnull
    {
      // FIXME
      // - Logging
      // - Authorization
      // - Auditing

      try
      {
        Logger.LogDebug("Execute action: {Action}", action.GetType().Name);

        using var trace = ActivitySource.StartActivity(action.GetType().Name);

        using (var scope = Services.CreateScope())
        {
          var unitOfWorkProvider = scope.ServiceProvider.GetRequiredService<IUnitOfWorkProvider<TContext>>();
          var context = CreateContext();
          await using (var uow = await unitOfWorkProvider.StartAsync(context))
          {
            var result = await actionHandler(scope.ServiceProvider);
            uow.Complete();
            return result;
          }
        }
      }
      catch (Exception ex)
      {
        string message = $"Failed to execute action '{action.GetType().Name}'.";
        if (ex is IDetailedException dex && dex.DetailedLogMessage != null)
          message += "\n" + dex.DetailedLogMessage;

        Logger.LogError(ex, message);

        throw;
      }
    }


    protected abstract TContext CreateContext();
  }
}
