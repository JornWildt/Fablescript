using Fablescript.Core.Contract.Engine;

namespace Fablescript.Core.GameConfiguration
{
  internal class LocationProvider : ILocationProvider
  {
    Task<Location> ILocationProvider.GetAsync(LocationId id)
    {
      if (id == TemporaryConstants.InitialLocationId)
        return Task.FromResult(new Location(
          TemporaryConstants.InitialLocationId,
          "Black sand beach",
          "You find yourself resting on the sand with no rememberence of how you came here or who you are.",
          [
            "The beach stretches as far as you can see to the east and west.",
            "Black sand covers all the beach.",
            "The sea is calm and stretches to infinity without interruptions.",
            "Soft mountains rises in the distance to the north. A huge glacier tops the central mountain."
          ]
          ));
      else
        throw new NotImplementedException();
    }
  }
}
