using Fablescript.Core.Contract.Engine;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Engine
{
  internal class Location : Entity<LocationId>
  {
    public string Title { get; private set; }

    public string? Introduction { get; private set; }
    
    public string[] Facts { get; private set; }

    public Exit[] Exits { get; private set; }


    public Location(
      LocationId id,
      string title,
      string? introduction,
      string[] facts,
      Exit[] exits)
      : base(id)
    {
      Title = title;
      Introduction = introduction;
      Facts = facts;
      Exits = exits;
    }


    public class Exit
    {
      public string Id { get; private set; }
      public string Name { get; private set; }
      public string? Description { get; private set; }
      public LocationId TargetLocationId { get; private set; }

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
