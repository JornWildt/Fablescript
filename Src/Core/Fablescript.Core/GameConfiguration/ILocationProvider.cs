using Fablescript.Core.Contract.Engine;

namespace Fablescript.Core.GameConfiguration
{
  public interface ILocationProvider
  {
    Task<Location> GetAsync(LocationId id);
  }
}
