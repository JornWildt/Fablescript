using Fablescript.Core.Contract.Engine;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Engine
{
  internal class Location : Entity<LocationId>
  {
    public string LocationName { get; set; }

    public string? Introduction { get; set; }
    
    public string[] Facts { get; set; }

    public Exit[] Exits { get; set; }


    public Location(
      LocationId id,
      string locationName,
      string? introduction,
      string[] facts,
      Exit[] exits)
      : base(id)
    {
      LocationName = locationName;
      Introduction = introduction;
      Facts = facts;
      Exits = exits;
    }


    public class Exit
    {
      public string Id { get; set; }
      public string Name { get; set; }
      public string? Description { get; set; }
      public LocationId TargetLocationId { get; set; }

      public Exit(string id, string name, string? description, LocationId targetLocationId)
      {
        Id = id;
        Name = name;
        Description = description;
        TargetLocationId = targetLocationId;
      }
    }
  }
}
