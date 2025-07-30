namespace Fablescript.Core.Engine
{
  public class Player
  {
    public ObjectId LocationId { get; set; }

    public Player(
      ObjectId locationId)
    {
      LocationId = locationId;
    }
  }
}
