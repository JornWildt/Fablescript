using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Utility.Base.Persistence;
using NLua;
using System.Dynamic;

namespace Fablescript.Core.Engine
{
  public class GameState : Entity<GameId>
  {
    public FableId FableId { get; private set; }

    public Player Player { get; private set; }


    private Lua RuntimeEnvironment { get; set; }


    private LuaTable ObjectPrototype { get; set; } = null!;

    private LuaFunction ObjectConstructor { get; set; } = null!;

    private LuaFunction ObjectInspector { get; set; } = null!;

    private IDictionary<ObjectId, LuaObject> Objects { get; }


    public GameState(
      GameId id,
      FableId fableId,
      Player player)
      : base(id)
    {
      FableId = fableId;
      Player = player;
      RuntimeEnvironment = new Lua();
      Objects = new Dictionary<ObjectId, LuaObject>();
    }


    public void Initialize()
    {
      RuntimeEnvironment.DoFile("D:\\External\\Fablescript\\Src\\Core\\Fablescript.Core\\Engine\\LuaInit.lua");

      // Get the GameObject prototype
      ObjectPrototype = (LuaTable)RuntimeEnvironment["GameObject"];

      // Get the constructor function
      ObjectConstructor = (LuaFunction)ObjectPrototype["new"];
      
      ObjectInspector = (LuaFunction)ObjectPrototype["Inspect"];
    }


    internal dynamic AddObject(ObjectId id, ExpandoObject src)
    {
      // Call GameObject:new{...} to create the object in Lua
      var table = (LuaTable)ObjectConstructor.Call(ObjectPrototype)[0];

      LuaObjectConverter.ConvertToLuaTable(RuntimeEnvironment, table, src);

      var obj = new LuaObject(table);
      Objects[id] = obj;

      return obj;
    }


    internal dynamic GetObject(ObjectId id)
    {
      return Objects[id];
    }


    internal IEnumerable<dynamic> GetAllObjects()
    {
      return Objects.Values.AsEnumerable();
    }
  }
}
