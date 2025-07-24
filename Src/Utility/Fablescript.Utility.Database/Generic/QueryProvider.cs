using Microsoft.EntityFrameworkCore;

namespace Fablescript.Utility.Database.Generic
{
  // FIXME: QueryProvider is an odd name.
  public class QueryProvider<TDataContext>
    where TDataContext : DbContext, IDataBaseContext
  {
    // To be implemented ...
  }
}
