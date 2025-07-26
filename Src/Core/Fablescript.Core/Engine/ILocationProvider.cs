using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;

namespace Fablescript.Core.Engine
{
  internal interface ILocationProvider
  {
    Task<Location> GetAsync(FableId fableId, LocationId locationId);
    Task<Location?> TryGetAsync(FableId fableId, LocationId locationId);
  }
}
