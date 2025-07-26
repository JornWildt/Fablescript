using Fablescript.Core;
using Fablescript.Core.Contract;
using Fablescript.Utility.Base.UnitOfWork;
using Fablescript.Utility.Services.CommandQuery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fablescript
{
  public static class MainConsoleFactoryExtensions
  {
    public static IServiceCollection AddMain(
      this IServiceCollection services,
      IConfiguration configuration)
    {
      services.AddScoped<IUnitOfWorkProvider<CoreUnitOfWorkContext>, ConsoleUnitOfWorkProvider<CoreUnitOfWorkContext>>();
      services.AddScoped<ICommandProcessor, CoreCommandProcessor>();
      //services.AddScoped<IQueryProcessor, CoreQueryProcessor>();

      return services;
    }
  }
}
