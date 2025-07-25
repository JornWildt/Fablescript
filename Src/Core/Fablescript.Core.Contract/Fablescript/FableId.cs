using Fablescript.Utility.Base;

namespace Fablescript.Core.Contract.Fablescript
{
  public class FableId : EntityId<string>
  {
    public FableId(string value)
      : base(value)
    {
    }
  }
}
