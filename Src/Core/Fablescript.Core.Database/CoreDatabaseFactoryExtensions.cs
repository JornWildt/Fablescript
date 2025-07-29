using Fablescript.Core.Database.Engine;
using Fablescript.Core.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fablescript.Core.Database
{
  public static class CoreDatabaseFactoryExtensions
  {
    public static IServiceCollection AddCorePersistence(
      this IServiceCollection services,
      IConfiguration configuration)
    {
      AddBasicCorePersistence(services, configuration);

      services.AddScoped<IPlayerRepository, PlayerRepository>();
      services.AddSingleton<IObjectRepository, ObjectRepository>();

      return services;
    }


    private static IServiceCollection AddBasicCorePersistence(
      IServiceCollection services,
      IConfiguration configuration)
    {
      //services.AddSingleton<IUnitOfWorkParticipant<CoreUnitOfWorkContext, CoreDataContext>, CoreUnitOfWorkParticipant>();
      //services.AddTransient<IDatabaseMigrator, DatabaseMigrator>();

      //services.AddDbContextFactory<CoreDataContext>((services, options) =>
      //{
      //  options
      //    .UseNpgsql() // Connection string will be assigned in every request by CoreUnitOfWorkParticipant.
      //    .UseLowerCaseNamingConvention();

      //  var cfg = services.GetService<CoreConfiguration>();
      //  if (cfg?.Database?.EnableEFLogging ?? false)
      //    options.UseLoggerFactory(services.GetRequiredService<ILoggerFactory>());
      //});


      return services;
    }
  }
}
