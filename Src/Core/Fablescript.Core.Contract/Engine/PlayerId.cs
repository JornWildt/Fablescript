using Fablescript.Utility.Base;

namespace Fablescript.Core.Contract.Engine
{
  public class PlayerId : EntityId<string>
  {
    public PlayerId(string value)
      : base(value)
    {
    }
  }
}
