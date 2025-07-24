using Fablescript.Core.Contract.Engine;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.GameConfiguration
{
  public class Location : Entity<LocationId>
  {
    public string LocationName { get; set; }

    public string Introduction { get; set; }
    public string[] Facts { get; set; }
    
    
    public Location(
      LocationId id,
      string locationName,
      string introduction,
      string[] facts)
      : base(id)
    {
      LocationName = locationName;
      Introduction = introduction;
      Facts = facts;
    }
  }
}
