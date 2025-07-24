using Fablescript.Utility.Base;

namespace Fablescript.Core.Contract.Engine
{
  public class PlayerId : EntityId<Guid>
  {
    public PlayerId(Guid value)
      : base(value)
    {
    }
  }
}
