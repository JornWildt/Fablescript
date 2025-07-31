using System.Dynamic;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Core.Fablescript;
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


    internal void Initialize()
    {
      // Get the BaseObject prototype
      ObjectPrototype = (LuaTable)RuntimeEnvironment["BaseObject"];

      // Get the constructor function
      ObjectConstructor = (LuaFunction)ObjectPrototype["new"];
      
      Player = CreateBaseObject();
      Player.Name = "Player";
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

    internal LuaObject CreateBaseObject()
    {
      var table = (LuaTable)ObjectConstructor.Call(ObjectPrototype)[0];
      return new LuaObject(table);
    }

    //internal LuaObject AddObject(ObjectId id, ExpandoObject src)
    //{
    //  // Call BaseObject:new{...} to create the object in Lua
    //  var table = (LuaTable)ObjectConstructor.Call(ObjectPrototype)[0];

    //  LuaConverter.CopyToLuaTable(RuntimeEnvironment, table, src);

    //  InvokeMethod(table, "inspect");

    //  var obj = new LuaObject(table);
    //  Objects[id] = obj;

    //  return obj;
    //}


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
      var method = (LuaFunction)self[methodName];
      var allArgs = new object[1 + args.Length];
      allArgs[0] = self;
      Array.Copy(args, 0, allArgs, 1, args.Length);
      method.Call(allArgs);
    }
    
    
    internal void InvokeFunction(string functionName, params object[] args)
    {
      var path = functionName.Split('.');
      var element = RuntimeEnvironment[path[0]];
      for (int i=1; i<path.Length; i++)
      {
        if (element is LuaTable t)
          element = t[path[i]];
      }
      var func = (LuaFunction)element;
      var result = func.Call(args);
    }
  }
}
