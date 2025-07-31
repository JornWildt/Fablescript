using NLua;
using System.Dynamic;

namespace Fablescript.Core.Engine
{
  internal class LuaObject : DynamicObject
  {
    private LuaTable Source { get; set; }

    
    public LuaObject(LuaTable source)
    {
      Source = source;
    }


    public override IEnumerable<string> GetDynamicMemberNames()
    {
      return Source.Keys.OfType<string>();
    }


    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
      var value = Source[binder.Name];

      // If the stored value is null, return a defaulting dynamic so value-type
      // conversions (e.g., to int) don’t throw.
      result = value == null ? DynamicValue.Null : new DynamicValue(value);
      return true;
    }


    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
      Source[binder.Name] = value;
      return true;
    }
  }
}
