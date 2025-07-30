using System.Collections.Concurrent;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Engine
{
  internal class GameState : Entity<GameId>
  {
    public FableId FableId { get; private set; }

    public Player Player { get; private set; }


    private IDictionary<ObjectId, Object> Objects { get; }


    public GameState(
      GameId id,
      FableId fableId,
      Player player)
      : base(id)
    {
      FableId = fableId;
      Player = player;

      Objects = new ConcurrentDictionary<ObjectId, Object>();
    }
  }
}
