using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Engine
{
  public class Player : Entity<PlayerId>
  {
    public FableId FableId { get; set; }

    public LocationId LocationId { get; set; }


    public Player(
      PlayerId id,
      FableId fableId,
      LocationId locationId)
      : base(id)
    {
      FableId = fableId;
      LocationId = locationId;
    }
  }
}
