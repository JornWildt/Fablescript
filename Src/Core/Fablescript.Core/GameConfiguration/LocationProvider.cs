using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Core.Fablescript;

namespace Fablescript.Core.GameConfiguration
{
  internal class LocationProvider : ILocationProvider
  {
    #region Dependencies

    private readonly IFablescriptParser FablescriptParser;

    #endregion


    public LocationProvider(IFablescriptParser fablescriptParser)
    {
      FablescriptParser = fablescriptParser;
    }

    
    async Task<Location> ILocationProvider.GetAsync(FableId fableId, LocationId locationId)
    {
      var location = await TryGetAsync(fableId, locationId);
      if (location == null)
        throw new InvalidOperationException($"No location '{locationId}' in fable '{fableId}'");
      return location;
    }

    
    Task<Location?> ILocationProvider.TryGetAsync(FableId fableId, LocationId locationId)
    {
      return TryGetAsync(fableId, locationId);
    }

    
    private async Task<Location?> TryGetAsync(FableId fableId, LocationId locationId)
    {
      var ldef = await FablescriptParser.TryGetAsync(fableId, locationId);
      if (ldef == null)
        return null;

      // Locations never changes, so conversion result could be cached, as long as the cache is cleared when everything is reloaded.
      var location = new Location(
        new LocationId(ldef.Name),
        ldef.Title,
        ldef.Introduction,
        ConvertFacts(ldef.Facts),
        ConvertExits(ldef.Exits));

      return location;
    }


    private string[] ConvertFacts(LocationFactDefinition[]? facts)
    {
      if (facts == null)
        return [];

      return facts
        .Select(f => f.Text)
        .ToArray();
    }


    private Location.Exit[] ConvertExits(LocationExitDefinition[]? exits)
    {
      if (exits == null)
        return [];

      return exits
        .Select(x => new Location.Exit(x.Name, x.Title, x.Description, new LocationId(x.TargetLocationName)))
        .ToArray();
    }
  }
}
