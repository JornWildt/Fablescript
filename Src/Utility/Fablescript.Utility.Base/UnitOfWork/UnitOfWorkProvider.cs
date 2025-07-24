using Microsoft.Extensions.Logging;

namespace Fablescript.Utility.Base.UnitOfWork
{
  public abstract class UnitOfWorkProvider<TContext> : IUnitOfWorkProvider<TContext>
    where TContext : IUnitOfWorkContext
  {
    #region Dependencies

    readonly IUnitOfWorkConfigurator<TContext> UnitOfWorkConfigurator;
    readonly ILoggerFactory LoggerFactory;

    #endregion


    public UnitOfWorkProvider(
      IUnitOfWorkConfigurator<TContext> unitOfWorkConfigurator,
      ILoggerFactory loggerFactory)
    {
      UnitOfWorkConfigurator = unitOfWorkConfigurator;
      LoggerFactory = loggerFactory;
    }


    #region Participants

    #endregion


    async Task<IUnitOfWork> IUnitOfWorkProvider<TContext>.StartAsync(
      TContext context,
      bool autoComplete)
    {
      if (Current == null)
      {
        var participantInvokers = UnitOfWorkConfigurator.Participants.Select(p => p.GetInvoker()).ToArray();
        var logger = LoggerFactory.CreateLogger<UnitOfWork<TContext>>();
        Current = new UnitOfWork<TContext>(context, participantInvokers, autoComplete, logger);
        Current.Completed += UnitOfWorkCompleted;
        await Current.StartAsync();
      }
      else
      {
        Current.AddReference();
      }
      return Current;
    }


    IUnitOfWork? IUnitOfWorkProvider<TContext>.Current => Current;


    private void UnitOfWorkCompleted(object? sender, EventArgs e)
    {
      Current = null;
    }


    protected abstract IUnitOfWork? Current { get; set; }
  }
}
