using Fablescript.Utility.Base;

namespace Fablescript.Core.Contract.Engine
{
  public class LocationId : EntityId<Guid>
  {
    public LocationId(Guid value)
      : base(value)
    {
    }
  }
}
