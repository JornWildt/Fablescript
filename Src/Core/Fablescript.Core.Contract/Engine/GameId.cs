using Fablescript.Utility.Base;

namespace Fablescript.Core.Contract.Engine
{
  public class GameId : EntityId<Guid>
  {
    public GameId(Guid value)
      : base(value)
    {
    }
  }
}
