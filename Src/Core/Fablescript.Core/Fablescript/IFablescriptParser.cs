using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;

namespace Fablescript.Core.Fablescript
{
  internal interface IFablescriptParser
  {
    FableDefinitionSet GetResult();
    Task<LocationDefinition?> TryGetAsync(FableId fableId, LocationId locationId);
  }
}
