using Fablescript.Core.Contract.Engine;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.GameConfiguration
{
  public class Location : Entity<LocationId>
  {
    public Location(LocationId id)
      : base(id)
    {
    }
  }
}
