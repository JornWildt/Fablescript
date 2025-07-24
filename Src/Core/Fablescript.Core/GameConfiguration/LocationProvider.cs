using Fablescript.Core.Contract.Engine;

namespace Fablescript.Core.GameConfiguration
{
  internal class LocationProvider : ILocationProvider
  {
    Task<Location> ILocationProvider.GetAsync(LocationId id)
    {
      if (id == TemporaryConstants.BlackBeachId)
        return Task.FromResult(new Location(
          id,
          "Black sand beach",
          "You find yourself resting on the sand with no rememberence of how you came here or who you are.",
          [
            "The beach stretches as far as you can see to the east and west.",
            "Black sand covers all the beach.",
            "The sea is calm and stretches to infinity without interruptions.",
            "Soft mountains rises in the distance to the north. A huge glacier tops the central mountain."
          ],
          [
            new Location.Exit("east", "East", "The beach stretches to the east.", TemporaryConstants.BlackBeachEastId)  
          ]
          ));
      else if (id == TemporaryConstants.BlackBeachEastId)
        return Task.FromResult(new Location(
          id,
          "Eastern end of black sand beach",
          "",
          [
            "The beach stretches as far as you can see to the west.",
            "Cliffs rises in the east",
            "Black sand covers all the beach.",
            "The sea is calm and stretches to infinity without interruptions.",
            "Soft mountains rises in the distance to the north. A huge glacier tops the central mountain."
          ],
          []
          ));

      else
        throw new NotImplementedException();
    }
  }
}
