using Fablescript.Utility.Base;
using NLua;
using System.Dynamic;

namespace Fablescript.Core.Engine
{
  internal static class LuaObjectConverter
  {
    public static LuaTable ConvertToLuaTable(Lua lua, ExpandoObject src)
    {
      var table = (LuaTable)lua.DoString("return {}")[0];
      return ConvertToLuaTable(lua, table, src);
    }


    public static LuaTable ConvertToLuaTable(Lua lua, LuaTable? table, ExpandoObject src)
    {
      table = table ?? (LuaTable)lua.DoString("return {}")[0];

      foreach (var item in src)
      {
        var value = item.Value;
        if (value is Array arr)
        {
          value = ConvertArrayToLuaTable(lua, arr);
        }
        else if (value is EntityId<Guid> eid)
        {
          value = eid.Value.ToString();
        }
        else if (value is Guid id)
        {
          value = id.ToString();
        }

        table[item.Key] = value;
      }

      return table;
    }


    public static LuaTable ConvertArrayToLuaTable(Lua lua, Array arr)
    {
      var table = (LuaTable)lua.DoString("return {}")[0];

      for (int i=0; i<arr.Length; ++i)
      {
        var item = arr.GetValue(i);
        if (item is ExpandoObject obj)
        {
          table[i] = ConvertToLuaTable(lua, table, obj);
        }
      }

      return table;
    }
  }
}
