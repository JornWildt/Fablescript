namespace Fablescript.Utility.Base.UnitOfWork
{
  /// <summary>
  /// A participant in unit of work.
  /// </summary>
  /// <remarks>Partcipants are registered with the unit of work configurator and passed to every new unit of work.</remarks>
  /// <typeparam name="TContext">Context data for the unit of work.</typeparam>
  /// <typeparam name="TPayload">Partcipant's own payload in the unit of work.</typeparam>
  public interface IUnitOfWorkParticipant<TContext, TPayload>
    where TContext : IUnitOfWorkContext
  {
    /// <summary>
    /// Notification of start of unit of work.
    /// </summary>
    /// <param name="context">Context data for the unit of work.</param>
    /// <returns>Participant's own payload data needed to keep track of the unit of work.</returns>
    Task<TPayload> StartAsync(TContext context);

    /// <summary>
    /// Commit work done by this partcipant.
    /// </summary>
    /// <param name="context">Context data for the unit of work.</param>
    /// <param name="payload">The payload data created by the participant at the start of the unit of work.</param>
    /// <returns></returns>
    Task CommitAsync(TContext context, TPayload payload);


    /// <summary>
    /// Rollback work done by this participant.
    /// </summary>
    /// <param name="context">Context data for the unit of work.</param>
    /// <param name="payload">The payload data created by the participant at the start of the unit of work.</param>
    /// <returns></returns>
    Task RollbackAsync(TContext context, TPayload payload);
  }
}
