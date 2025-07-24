namespace Fablescript.Utility.Base.Persistence
{
  public abstract class Entity<TId> : IEntity<TId>
    where TId : notnull
  {
    public TId Id { get; private init; }

    public Entity(TId id)
    {
      Id = id;
    }
  }
}
