using Fablescript.Core.Contract.Engine;

namespace Fablescript.Core.GameConfiguration
{
  public interface ILocationProvider
  {
    Location Get(LocationId id);
  }
}
