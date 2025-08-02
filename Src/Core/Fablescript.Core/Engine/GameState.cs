using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Utility.Base.Persistence;
using NLua;

namespace Fablescript.Core.Engine
{
  public class GameState : Entity<GameId>
  {
    public FableId FableId { get; private set; }


    private Lua RuntimeEnvironment { get; set; }

    internal dynamic Player { get; set; } = null!;

    private LuaTable ObjectPrototype { get; set; } = null!;

    private LuaFunction ObjectConstructor { get; set; } = null!;

    private IDictionary<ObjectId, LuaObject> Objects { get; }

    public List<string> ResponseOutput { get; private init; }


    public GameState(
      GameId id,
      FableId fableId,
      string coreLuaDir)
      : base(id)
    {
      FableId = fableId;
      RuntimeEnvironment = new Lua();
      RuntimeEnvironment.DoString($"package.path = '{coreLuaDir}; ' .. package.path");
      Objects = new Dictionary<ObjectId, LuaObject>();
      ResponseOutput = new List<string>();
    }


    internal void Initialize()
    {
      // Get the BaseObject prototype
      ObjectPrototype = (LuaTable)RuntimeEnvironment["BaseObject"];

      // Get the constructor function
      ObjectConstructor = (LuaFunction)ObjectPrototype["new"];
    }


    internal LuaTable CreateEmptyLuaTable()
    {
      var table = (LuaTable)RuntimeEnvironment.DoString("return {}")[0];
      return table;
    }


    internal LuaTable CreateLuaArray(IEnumerable<LuaTable> items)
    {
      var arr = CreateEmptyLuaTable();
      int i = 1;
      foreach (LuaTable item in items)
      {
        arr[i] = item;
        ++i;
      }
      return arr;
    }


    internal void AddFunction(string? ns, string name, object f)
    {
      if (ns != null)
      {
        var nsObj = RuntimeEnvironment[ns] as LuaTable;
        if (nsObj == null)
        {
          nsObj = CreateEmptyLuaTable();
          RuntimeEnvironment[ns] = nsObj;
        }
        nsObj[name] = f;
      }
      else
      {
        RuntimeEnvironment[name] = f;
      }
    }


    internal LuaObject CreateBaseObject(string? ns, string name)
    {
      var table = (LuaTable)ObjectConstructor.Call(ObjectPrototype)[0];

      if (ns != null)
      {
        var nsObj = RuntimeEnvironment[ns] as LuaTable;
        if (nsObj == null)
        {
          nsObj = CreateEmptyLuaTable();
          RuntimeEnvironment[ns] = nsObj;
        }
        nsObj[name] = table;
      }
      else
      {
        RuntimeEnvironment[name] = table;
      }

      return new LuaObject(table);
    }


    internal LuaObject GetObject(ObjectId id)
    {
      return Objects[id];
    }


    internal IEnumerable<dynamic> GetAllObjects()
    {
      return Objects.Values.AsEnumerable();
    }


    internal void LoadScript(string filename)
    {
      RuntimeEnvironment.DoString($"require('{filename}')");
    }


    internal void InvokeMethod(LuaTable self, string methodName, params object[] args)
    {
      ResponseOutput.Clear();

      var method = (LuaFunction)self[methodName];
      var allArgs = new object[1 + args.Length];
      allArgs[0] = self;
      Array.Copy(args, 0, allArgs, 1, args.Length);
      method.Call(allArgs);
    }


    internal object? InvokeFunction(string functionName, object?[] args)
    {
      ResponseOutput.Clear();

      var path = functionName.Split('.');
      var element = RuntimeEnvironment[path[0]];
      for (int i = 1; i < path.Length; i++)
      {
        if (element is LuaTable t)
          element = t[path[i]];
      }
      var func = (LuaFunction)element;
      if (func != null)
      {
        var result = func.Call(args);
        if (result is not null && result.Length > 0)
          return result[0];
        return null;
      }
      else
      {
        Console.WriteLine("Unknown function: " + functionName);
        return null;
      }
    }
  }
}
