using Fablescript.Core.Contract.Engine;

namespace Fablescript.Core.GameConfiguration
{
  internal class LocationProvider : ILocationProvider
  {
    Location ILocationProvider.Get(LocationId id)
    {
      if (id == TemporaryConstants.InitialLocationId)
        return new Location(TemporaryConstants.InitialLocationId);
      else
        throw new NotImplementedException();
    }
  }
}
