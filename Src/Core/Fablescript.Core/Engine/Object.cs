using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Engine
{
  public class Object : Entity<ObjectId>
  {
    public string Name { get; set; }

    public string Title { get; set; }

    public string? Description { get; set; }


    public Object(
      ObjectId id,
      string name,
      string title,
      string? description)
      : base(id)
    {
      Name = name;
      Title = title;
      Description = description;
    }
  }
}
