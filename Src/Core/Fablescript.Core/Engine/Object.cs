using Fablescript.Core.Contract.Engine;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Engine
{
  public class Object : Entity<ObjectId>
  {
    // Consider this as game ID - it is not carried by the player, but exists in the player's game
    // FIXME: Implement GameId
    public PlayerId PlayerId { get; set; }

    public string Name { get; set; }

    public string Title { get; set; }

    public string? Description { get; set; }

    public LocationId? Location { get; set; }


    public Object(
      ObjectId id,
      PlayerId playerId,
      string name,
      string title,
      string? description,
      LocationId? location)
      : base(id)
    {
      PlayerId = playerId;
      Name = name;
      Title = title;
      Description = description;
      Location = location;
    }
  }
}
