using Fablescript.Core.Contract.Engine;
using Fablescript.Utility.Base.Persistence;
using System.Collections.Concurrent;
using System.Dynamic;

namespace Fablescript.Core.Engine
{
  public class FableObject : DynamicObject, IEntity<ObjectId>
  {
    public ObjectId Id
    {
      set => Properties["Id"] = value;
      get => (ObjectId)Properties["Id"]!;
    }

    public GameId GameId { get; private init; }

    private ConcurrentDictionary<string, dynamic?> Properties { get; set; }


    public FableObject(
      ObjectId id,
      GameId gameId)
    {
      Properties = new ConcurrentDictionary<string, dynamic?>();
      Id = id;
      GameId = gameId;
    }


    public override IEnumerable<string> GetDynamicMemberNames()
    {
      return Properties.Keys;
    }


    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
      if (Properties.TryGetValue(binder.Name, out var value))
      {
        // If the stored value is null, return a defaulting dynamic so value-type
        // conversions (e.g., to int) don’t throw.
        result = value == null ? DynamicValue.Null : new DynamicValue(value);
        return true;
      }

      // Missing member: return a defaulting dynamic that can convert to whatever
      // the context requires (int → 0, bool → false, string/reference → null, etc.).
      result = DynamicValue.Null;
      return true;
    }


    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
      Properties[binder.Name] = value;
      return true;
    }
  }
}
