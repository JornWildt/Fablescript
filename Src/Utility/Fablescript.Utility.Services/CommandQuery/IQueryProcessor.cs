using Fablescript.Utility.Services.Contract.CommandQuery;

namespace Fablescript.Utility.Services.CommandQuery
{
  public interface IQueryProcessor
  {
    Task<TResult> InvokeQueryAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>;
  }

  
  public interface IQueryProcessor<T> : IQueryProcessor
  {
  }
}
