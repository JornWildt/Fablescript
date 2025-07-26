using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;

namespace Fablescript.Core.Engine
{
  internal record FableDTO(
    FableId Id,
    string Title,
    string Description,
    LocationId InitialLocation);
}
