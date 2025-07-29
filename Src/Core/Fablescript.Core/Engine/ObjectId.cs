using Fablescript.Utility.Base;

namespace Fablescript.Core.Engine
{
  public class ObjectId : EntityId<Guid>
  {
    public ObjectId(Guid value)
      : base(value)
    {
    }
  }
}
