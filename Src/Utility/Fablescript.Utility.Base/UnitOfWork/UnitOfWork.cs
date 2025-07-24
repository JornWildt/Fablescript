using Fablescript.Utility.Base.UnitOfWork.Internal;
using Microsoft.Extensions.Logging;

namespace Fablescript.Utility.Base.UnitOfWork
{
  /// <summary>
  /// Generic implementation of a unit of work which will notify participants of unit of work events (start, commit, rollback).
  /// </summary>
  /// <typeparam name="TContext">Context data type for the unit of work, passed to partcipants.</typeparam>
  public class UnitOfWork<TContext> : IUnitOfWork
    where TContext : IUnitOfWorkContext
  {
    protected TContext Context { get; }
    protected IReadOnlyList<IParticipantInvoker<TContext>> Participants { get; }

    ILogger<UnitOfWork<TContext>> Logger { get; }

    protected bool IsComplete { get; set; }

    // Make it possible to nest unit of work.
    // All nested units of work must complete successfully for the top most unit of work to commit work.
    // Expect successfull CompleteCount to match the number of references to the unit of work.
    protected int ReferenceCount { get; set; }
    protected int CompleteCount { get; set; }


    public UnitOfWork(
      TContext context,
      IReadOnlyList<IParticipantInvoker<TContext>> participants,
      bool autoComplete,
      ILogger<UnitOfWork<TContext>> logger)
    {
      logger.LogDebug("Create unit of work {Value} for {Context}.", context, typeof(TContext));

      Context = context;
      Participants = participants;
      Logger = logger;

      IsComplete = autoComplete;

      // A new unit of work has one reference to it and expects one successfull complete to commit work.
      ReferenceCount = 1;
      CompleteCount = 1;
    }


    public event EventHandler? Completed;

    protected void OnCompleted()
    {
      Completed?.Invoke(this, EventArgs.Empty);
    }
    
    
    async Task IUnitOfWork.StartAsync()
    {
      foreach (var p in Participants)
      {
        Logger.LogDebug("Start {Participant} for {Context}.", p, typeof(TContext));
        await p.StartAsync(Context);
      }
    }


    void IUnitOfWork.AddReference()
    {
      // When nesting units of work, keep count of the nesting level (reference count) and how
      // many successfull completes are needed to commit work.
      ++ReferenceCount;
      ++CompleteCount;
    }

    
    void IUnitOfWork.Complete()
    {
      // When all units of work are completed successfully then mark unit of work as fully complete.
      --CompleteCount;
      if (CompleteCount == 0)
        IsComplete = true;
    }


    TPayload IUnitOfWork.Payload<TPayload>()
    {
      foreach (var p in Participants)
      {
        if (p.TryGetPayload(out TPayload? payload))
        {
          Logger.LogDebug("Got payload {Payload} for {Context}.", payload, typeof(TContext));
          return payload!;
        }
      }

      throw new InvalidOperationException($"No payload of type {typeof(TPayload)} found in unit of work.");
    }


    async ValueTask IAsyncDisposable.DisposeAsync()
    {
      // Do not try to complete the unit of work before all references to it are disposed.
      if (--ReferenceCount > 0)
        return;

      try
      {
        if (IsComplete)
        {
          Logger.LogDebug("Commit all for {Context}.", typeof(TContext));
          await CommitAll();
        }
        else
        {
          Logger.LogDebug("Rollback all for {Context}.", typeof(TContext));
          await RollbackAll();
        }
      }
      finally
      {
        OnCompleted();
      }
    }

    
    private async Task CommitAll()
    {
      Exception? failed = null;

      foreach (var p in Participants)
      {
        try
        {
          if (failed == null)
            await p.CommitAsync(Context);
          else
            await p.RollbackAsync(Context);
        }
        catch (Exception ex)
        {
          failed = ex;
        }
      }

      if (failed != null)
      {
        Logger.LogError(failed, "Not all unit of work participants commited. Some were rolled back after an exception.");
        throw new InvalidOperationException("Not all unit of work participants commited. Some were rolled back after an exception.", failed);
      }
    }


    private async Task RollbackAll()
    {
      Exception? failed = null;

      foreach (var p in Participants)
      {
        try
        {
          await p.RollbackAsync(Context);
        }
        catch (Exception ex)
        {
          Logger.LogError(ex, "Unit of work rollback failed.");
          failed = ex;
        }
      }

      if (failed != null)
      {
        throw new InvalidOperationException("Unit of work rollback failed.", failed);
      }
    }
  }
}
