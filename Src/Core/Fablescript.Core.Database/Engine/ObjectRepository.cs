using System.Collections.Concurrent;
using System.Numerics;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Engine;
using Fablescript.Utility.Base.Persistence;
using Object = Fablescript.Core.Engine.Object;

namespace Fablescript.Core.Database.Engine
{
  public class ObjectRepository : IObjectRepository
  {
    // FIXME: Concurrency!
    private static IDictionary<ObjectId, Object> Objects { get; }
    private static IDictionary<PlayerId, IList<Object>> PlayerObjects { get; }

    static ObjectRepository()
    {
      // FIXME: Use persistent database
      Objects = new ConcurrentDictionary<ObjectId, Object>();
      PlayerObjects = new ConcurrentDictionary<PlayerId, IList<Object>>();
    }

    Task<IReadOnlyList<Object>> IObjectRepository.GetAllObjectsAsync(PlayerId playerId)
    {
      if (!PlayerObjects.TryGetValue(playerId, out var objList))
      {
        objList = new List<Object>();
      }

      return Task.FromResult<IReadOnlyList<Object>>(objList.AsReadOnly());
    }

    void IRepository<Core.Engine.Object, ObjectId>.Add(Core.Engine.Object entity)
    {
      throw new NotImplementedException();
    }

    Task IRepository<Core.Engine.Object, ObjectId>.AddAsync(Core.Engine.Object obj)
    {
      Objects.TryAdd(obj.Id, obj);
      if (!PlayerObjects.TryGetValue(obj.PlayerId, out var objList))
      {
        objList = new List<Object>();
        PlayerObjects.TryAdd(obj.PlayerId, objList);
      }
      objList.Add(obj);
      return Task.CompletedTask;
    }

    Core.Engine.Object IRepository<Core.Engine.Object, ObjectId>.Get(ObjectId id)
    {
      throw new NotImplementedException();
    }

    IReadOnlyList<Core.Engine.Object> IRepository<Core.Engine.Object, ObjectId>.GetAll()
    {
      throw new NotImplementedException();
    }

    Task<IReadOnlyList<Core.Engine.Object>> IRepository<Core.Engine.Object, ObjectId>.GetAllAsync()
    {
      throw new NotImplementedException();
    }

    Task<Core.Engine.Object> IRepository<Core.Engine.Object, ObjectId>.GetAsync(ObjectId id)
    {
      throw new NotImplementedException();
    }

    void IRepository<Core.Engine.Object, ObjectId>.Remove(ObjectId id)
    {
      throw new NotImplementedException();
    }

    Task IRepository<Core.Engine.Object, ObjectId>.RemoveAsync(ObjectId id)
    {
      throw new NotImplementedException();
    }

    bool IRepository<Core.Engine.Object, ObjectId>.TryGet(ObjectId id, out Core.Engine.Object entity)
    {
      throw new NotImplementedException();
    }

    Task<(bool Success, Core.Engine.Object? Entity)> IRepository<Core.Engine.Object, ObjectId>.TryGetAsync(ObjectId id)
    {
      throw new NotImplementedException();
    }
  }
}
