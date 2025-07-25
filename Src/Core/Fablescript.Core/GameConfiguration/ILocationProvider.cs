using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;

namespace Fablescript.Core.GameConfiguration
{
  public interface ILocationProvider
  {
    Task<Location> GetAsync(FableId fableId, LocationId locationId);
    Task<Location?> TryGetAsync(FableId fableId, LocationId locationId);
  }
}
