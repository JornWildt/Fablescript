namespace Fablescript.Utility.Base.UnitOfWork
{
  /// <summary>
  /// Unit of work provider.
  /// </summary>
  /// <typeparam name="TContext">Context data type for the unit of work, passed to partcipants.</typeparam>
  public interface IUnitOfWorkProvider<TContext>
    where TContext : IUnitOfWorkContext
  {
    /// <summary>
    /// Create a new unit of work and start it.
    /// </summary>
    /// <param name="context">Context instance for the unit of work. Will be passed to every participant.</param>
    /// <param name="autoComplete">Make the unit of work complete successfully automatically without the need to call Complete() on the unit of work.</param>
    /// <returns></returns>
    Task<IUnitOfWork> StartAsync(TContext context, bool autoComplete = false);

    /// <summary>
    /// Current unit of work.
    /// </summary>
    IUnitOfWork? Current { get; }
  }
}
