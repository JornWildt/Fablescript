using System.Diagnostics.CodeAnalysis;

namespace Fablescript.Utility.Base.UnitOfWork.Internal
{
  /// <summary>
  /// Simplified interface for unit of work participants. It removes the need for including the generic payload type in the interface type.
  /// </summary>
  /// <typeparam name="TContext">Unit of work context to pass to partcipants.</typeparam>
  public interface IParticipantInvoker<TContext>
  {
    Task StartAsync(TContext context);
    Task CommitAsync(TContext context);
    Task RollbackAsync(TContext context);
    bool TryGetPayload<TPayload>([MaybeNullWhen(false)] out TPayload payload);
  }
}
