using Fablescript.Utility.Base;

namespace Fablescript.Core.Contract.Engine
{
  public class LocationId : EntityId<string>
  {
    public LocationId(string value)
      : base(value)
    {
    }
  }
}
