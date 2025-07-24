using System.Security.Cryptography.X509Certificates;
using Fablescript.Core.Contract.Engine;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Engine
{
  public class Player : Entity<PlayerId>
  {
    public LocationId Location { get; set; }


    public Player(
      PlayerId id,
      LocationId location)
      : base(id)
    {
      Location = location;
    }
  }
}
