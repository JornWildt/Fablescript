using Fablescript.Core.Contract;
using Fablescript.Core.Engine;
using Fablescript.Core.GameConfiguration;
using Fablescript.Utility.Base.UnitOfWork;
using Fablescript.Utility.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fablescript.Core
{
  public static class CoreFactoryExtensions
  {
    public static IServiceCollection AddCore(
      this IServiceCollection services,
      IConfiguration configuration)
    {
      services.AddAllServiceInterfaces<FableEngine>(asSingleton: false);
      services.AddSingleton<IGameProvider, GameProvider>();
      services.AddSingleton<ILocationProvider, LocationProvider>();

      services.AddSingleton<IUnitOfWorkConfigurator<CoreUnitOfWorkContext>, UnitOfWorkConfigurator<CoreUnitOfWorkContext>>();
      //services.AddVerifiedConfiguration<CoreConfiguration>(configuration, "Core");

      return services;
    }
  }
}
