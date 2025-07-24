using Fablescript.Utility.Services.Contract.CommandQuery;

namespace Fablescript.Utility.Services.CommandQuery
{
  public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
  {
    Task<TResult> InvokeAsync(TQuery query);
  }
}
