using Fablescript.Core.Contract.Engine;

namespace Fablescript.Core.GameConfiguration
{
  public static class TemporaryConstants
  {
    public static readonly PlayerId PlayerId = new PlayerId("Player-1");
    
    public static readonly LocationId BlackBeachId = new LocationId("BlackBeach");
    public static readonly LocationId BlackBeachEastId = new LocationId("BlackBeachEast");
    
    public static readonly LocationId InitialLocationId = BlackBeachId;
  }
}
