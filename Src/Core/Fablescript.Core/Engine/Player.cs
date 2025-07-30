namespace Fablescript.Core.Engine
{
  public class Player
  {
    public LocationId LocationId { get; set; }

    public Player(
      LocationId locationId)
    {
      LocationId = locationId;
    }
  }
}
