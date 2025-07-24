namespace Fablescript.Utility.Base.Persistence
{
  public interface IEntity<TId>
    where TId : notnull
  {
    TId Id { get; }
  }
}
