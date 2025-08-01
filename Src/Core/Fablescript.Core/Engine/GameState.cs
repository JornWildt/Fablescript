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

    internal dynamic Player { get; private set; } = null!;

    private LuaTable ObjectPrototype { get; set; } = null!;

    private LuaFunction ObjectConstructor { get; set; } = null!;

    private IDictionary<ObjectId, LuaObject> Objects { get; }



    public GameState(
      GameId id,
      FableId fableId)
      : base(id)
    {
      FableId = fableId;
      RuntimeEnvironment = new Lua();
      Objects = new Dictionary<ObjectId, LuaObject>();
    }


    internal void Initialize(LuaObject player)
    {
      // Get the BaseObject prototype
      ObjectPrototype = (LuaTable)RuntimeEnvironment["BaseObject"];

      // Get the constructor function
      ObjectConstructor = (LuaFunction)ObjectPrototype["new"];

      Player = player;
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
      RuntimeEnvironment.DoFile(filename);
    }


    internal void InvokeMethod(LuaTable self, string methodName, params object[] args)
    {
      InvokeWithOutput(() =>
      {
        var method = (LuaFunction)self[methodName];
        var allArgs = new object[1 + args.Length];
        allArgs[0] = self;
        Array.Copy(args, 0, allArgs, 1, args.Length);
        method.Call(allArgs);
      });
    }


    internal void InvokeFunction(string functionName, object?[] args)
    {
      InvokeWithOutput(() =>
      {
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
          func.Call(args);
        }
        else
          Console.WriteLine("Unknown function: " + functionName);
      });
    }


    void InvokeWithOutput(Action f)
    {
      ResponseOutput.Clear();
      f();
    }
  }
}
