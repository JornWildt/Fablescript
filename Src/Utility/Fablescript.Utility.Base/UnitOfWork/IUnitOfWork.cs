namespace Fablescript.Utility.Base.UnitOfWork
{
  public interface IUnitOfWork : IAsyncDisposable
  {
    /// <summary>
    /// Start unit of work. This will invoke the corresponding start method on all participants.
    /// </summary>
    /// <returns></returns>
    Task StartAsync();

    /// <summary>
    /// Keep track of additional references to this unit of work.
    /// </summary>
    /// <remarks>A unit of work must only be started once. But nested use of the same unit of work is allowed by adding references to it.</remarks>
    void AddReference();

    /// <summary>
    /// Mark unit of work as complete. All refering parties must mark the unit of work as complete before it is considered fully complete.
    /// </summary>
    void Complete();

    event EventHandler Completed;
    
    /// <summary>
    /// Get a specific type of payload associated with the unit of work. The payload comes from a participant that creates it.
    /// </summary>
    /// <remarks>Different participants cannot depend on the same payload type.</remarks>
    /// <typeparam name="TPayload">Payload type.</typeparam>
    /// <returns></returns>
    TPayload Payload<TPayload>();
  }
}
