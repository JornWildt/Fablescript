using Fablescript.Utility.Base;
using NLua;
using System.Collections;
using System.Dynamic;

namespace Fablescript.Core.Engine
{
  internal static class LuaConverter
  {
    public static LuaTable ConvertToLuaTable(Lua lua, ExpandoObject src)
    {
      var table = (LuaTable)lua.DoString("return {}")[0];
      return CopyToLuaTable(lua, table, src);
    }


    public static LuaTable CopyToLuaTable(Lua lua, LuaTable? table, ExpandoObject src)
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

      for (int i = 0; i < arr.Length; ++i)
      {
        var item = arr.GetValue(i);
        if (item is ExpandoObject obj)
        {
          table[i] = ConvertToLuaTable(lua, obj);
        }
      }

      return table;
    }


    public static IEnumerable<dynamic> ConvertLuaTableToEnumerable(LuaTable table)
    {
      foreach (var item in table.Values)
      {
        yield return new LuaObject((LuaTable)item);
      }
    }


    public static object ConvertLuaTableToDictionaryOrList(LuaTable table)
    {
      var kind = DetectLuaTableKind(table);
      if (kind == LuaTableKind.Array)
      {
        var result = new List<object?>();
        for (int i = 1; table[i] != null; ++i)
        {
          var value = ConvertLuaValueToObject(table[i]);
          result.Add(value);
        }
        return result;
      }
      else
      {
        var result = new Dictionary<string, object?>();
        foreach (KeyValuePair<object, object?> item in table)
        {
          if (item.Key != null)
          {
            var value = ConvertLuaValueToObject(item.Value);
            result[item.Key.ToString()!] = value;
          }
        }
        return result;
      }
    }


    public static object? ConvertLuaValueToObject(object? value)
    {
      if (value == null)
        return null;
      else if (value is LuaTable t)
        return ConvertLuaTableToDictionaryOrList(t);
      else
        return value;
    }
    
    
    internal enum LuaTableKind
    {
      Array,
      Map,
      Mixed
    }

    
    public static LuaTableKind DetectLuaTableKind(LuaTable table)
    {
      bool hasStringKey = false;
      bool hasIntKey = false;

      var numericKeys = new List<long>();

      foreach (KeyValuePair<object, object?> entry in table)
      {
        var key = entry.Key;

        if (key is string)
        {
          hasStringKey = true;
        }
        else if (key is long d) // Lua numbers as integers
        {
          hasIntKey = true;
          numericKeys.Add(d);
        }
        else
        {
          hasStringKey = true; // Fallback: treat other key types as map
        }
      }

      // Check if numeric keys are sequential 1..N
      if (hasIntKey && !hasStringKey)
      {
        numericKeys.Sort();
        for (int i = 0; i < numericKeys.Count; i++)
        {
          if (numericKeys[i] != i + 1)
            return LuaTableKind.Mixed;
        }
        return LuaTableKind.Array;
      }

      if (hasStringKey && !hasIntKey)
        return LuaTableKind.Map;

      return LuaTableKind.Mixed;
    }
  }
}
