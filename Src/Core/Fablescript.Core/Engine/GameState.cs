using System.Collections.Concurrent;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Utility.Base.Persistence;
using NLua;

namespace Fablescript.Core.Engine
{
  public class GameState : Entity<GameId>
  {
    public FableId FableId { get; private set; }

    public Player Player { get; private set; }


    private Lua RuntimeEnvironment { get; set; }

    private IDictionary<ObjectId, FableObject> Objects { get; }


    public GameState(
      GameId id,
      FableId fableId,
      Player player)
      : base(id)
    {
      FableId = fableId;
      Player = player;
      RuntimeEnvironment = new Lua();
      Objects = new Dictionary<ObjectId, FableObject>();
    }


    public void AddObject(ObjectId id, FableObject obj)
    {
      Objects.TryAdd(id, obj);
    }

    
    public FableObject GetObject(ObjectId id)
    {
      return Objects[id];
    }


    public IEnumerable<dynamic> GetAllObjects()
    {
      return Objects.Values.Cast<dynamic>().AsEnumerable();
    }
  }
}
