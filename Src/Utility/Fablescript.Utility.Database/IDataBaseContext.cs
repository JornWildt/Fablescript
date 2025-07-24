using Microsoft.EntityFrameworkCore.Storage;

namespace Fablescript.Utility.Database
{
  public interface IDataBaseContext
  {
    Task<IDbContextTransaction> BeginTransactionAsync();
  }
}
