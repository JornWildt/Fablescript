using Fablescript.Core.Contract.Engine;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Engine
{
  public interface IObjectRepository : IRepository<Object, ObjectId>
  {
    Task<IReadOnlyList<Object>> GetAllObjectsAsync(PlayerId playerId);
  }
}
