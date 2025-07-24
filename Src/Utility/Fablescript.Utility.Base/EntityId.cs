namespace Fablescript.Utility.Base
{
  public abstract class EntityId<T> : ValueObject
    where T : notnull
  {
    public T Value { get; }


    public EntityId(T value)
    {
      ArgumentNullException.ThrowIfNull(value, nameof(value));
      Value = value;
    }


    protected override IEnumerable<object> GetEqualityComponents()
    {
      yield return Value;
    }


    public override string ToString()
    {
      return Value?.ToString() ?? "<empty>";
    }
  }
}
