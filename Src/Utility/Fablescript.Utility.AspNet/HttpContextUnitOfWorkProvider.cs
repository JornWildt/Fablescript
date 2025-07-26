using Fablescript.Utility.Base.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fablescript.Utility.AspNet
{
  public class HttpContextUnitOfWorkProvider<TContext> : UnitOfWorkProvider<TContext>
    where TContext : IUnitOfWorkContext
  {
    #region Dependencies

    readonly IHttpContextAccessor HttpContextAccessor;

    #endregion


    const string CurrentUnitOfWorkKey = "CurrentUnitOfWorkKey";


    public HttpContextUnitOfWorkProvider(
      IHttpContextAccessor httpContextAccessor,
      IUnitOfWorkConfigurator<TContext> unitOfWorkConfigurator,
      ILoggerFactory loggerFactory)
      : base(unitOfWorkConfigurator, loggerFactory)
    {
      HttpContextAccessor = httpContextAccessor;
    }



    protected override IUnitOfWork? Current 
    {
      set
      {
        var context = HttpContextAccessor.HttpContext ?? throw new InvalidOperationException($"No HTTP context available");
        context.Items[CurrentUnitOfWorkKey] = value;
      }
      get
      {
        var context = HttpContextAccessor.HttpContext ?? throw new InvalidOperationException($"No HTTP context available");
        if (!context.Items.TryGetValue(CurrentUnitOfWorkKey, out var currentUnitOfWorkObj) || currentUnitOfWorkObj == null)
        {
          return null;
        }

        return (IUnitOfWork)currentUnitOfWorkObj;
      }
    }
  }
}
