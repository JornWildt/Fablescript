using System.Collections.Concurrent;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Engine
{
  public class GameState : Entity<GameId>
  {
    public FableId FableId { get; private set; }

    public Player Player { get; private set; }


    private IDictionary<ObjectId, FableObject> Objects { get; }


    public GameState(
      GameId id,
      FableId fableId,
      Player player,
      IReadOnlyCollection<FableObject> objects)
      : base(id)
    {
      FableId = fableId;
      Player = player;

      Objects = new ConcurrentDictionary<ObjectId, FableObject>(
        objects.Select(o => KeyValuePair.Create(o.Id, o)));
    }


    public Task<FableObject> GetObjectAsync(ObjectId id)
    {
      return Task.FromResult(Objects[id]);
    }


    public Task<IEnumerable<dynamic>> GetAllObjectsAsync()
    {
      return Task.FromResult(Objects.Values.Cast<dynamic>().AsEnumerable());
    }
  }
}
