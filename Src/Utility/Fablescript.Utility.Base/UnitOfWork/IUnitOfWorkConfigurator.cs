using Fablescript.Utility.Base.UnitOfWork.Internal;

namespace Fablescript.Utility.Base.UnitOfWork
{
  /// <summary>
  /// This interface hides the participant's payload type which is irrelevant for the unit of work provider.
  /// </summary>
  public interface IParticipantReference<TContext>
    where TContext : IUnitOfWorkContext
  {
    IParticipantInvoker<TContext> GetInvoker();
  }


  /// <summary>
  /// Configurator for unit of work.
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  public interface IUnitOfWorkConfigurator<TContext>
    where TContext : IUnitOfWorkContext
  {
    /// <summary>
    /// Register a participant for the provided unit of work instances.
    /// </summary>
    /// <typeparam name="TPayload">Partcipant's own payload in the unit of work.</typeparam>
    /// <param name="participant">Unit of work participant.</param>
    void RegisterParticipant<TPayload>(IUnitOfWorkParticipant<TContext, TPayload> participant);

    IReadOnlyList<IParticipantReference<TContext>> Participants { get; }
  }
}
