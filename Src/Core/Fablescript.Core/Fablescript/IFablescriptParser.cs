using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;

namespace Fablescript.Core.Fablescript
{
  internal interface IFablescriptParser
  {
    Task<FableDefinition> GetFableAsync(FableId fableId);
    Task<LocationDefinition?> TryGetLocationAsync(FableId fableId, LocationId locationId);
  }
}
