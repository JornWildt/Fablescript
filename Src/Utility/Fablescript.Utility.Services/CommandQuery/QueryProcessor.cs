using Fablescript.Utility.Base.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Fablescript.Utility.Services.CommandQuery
{
  public abstract class QueryProcessor<TContext> : BaseProcessor<TContext>,
    IQueryProcessor,
    IQueryProcessor<TContext>
    where TContext : IUnitOfWorkContext
  {
    #region Dependencies
    #endregion


    public QueryProcessor(
      //IAuditService auditService,
      //IIdentityProvider identityProvider,
      //ICommandValidator validator,
      ActivitySource activitySource,
      IServiceProvider serviceProvider,
      ILogger<QueryProcessor<TContext>> logger)
      : base(activitySource, serviceProvider, logger)
    {
    }


    async Task<TResult> IQueryProcessor.InvokeQueryAsync<TQuery, TResult>(TQuery query)
    {
      return await InvokeAsync(async services =>
        {
          var handler = services.GetRequiredService<IQueryHandler<TQuery, TResult>>();
          var result = await handler.InvokeAsync(query);
          return result;
        },
        query);
    }
  }
}
