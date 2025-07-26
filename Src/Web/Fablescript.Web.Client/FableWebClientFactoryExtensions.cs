using Fablescript.Core;
using Fablescript.Core.Contract;
using Fablescript.Utility.AspNet;
using Fablescript.Utility.Base.UnitOfWork;
using Fablescript.Utility.Services.CommandQuery;

namespace Fablescript.Web.Client
{
  public static class FableWebClientFactoryExtensions
  {
    public static IServiceCollection AddMainWebClient(
      this IServiceCollection services,
      IConfiguration configuration)
    {
      services.AddScoped<IUnitOfWorkProvider<CoreUnitOfWorkContext>, HttpContextUnitOfWorkProvider<CoreUnitOfWorkContext>>();
      services.AddScoped<ICommandProcessor, CoreCommandProcessor>();
      //services.AddScoped<IQueryProcessor, CoreQueryProcessor>();

      return services;
    }
  }
}
