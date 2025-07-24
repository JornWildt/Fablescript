using Microsoft.Extensions.DependencyInjection;

namespace Fablescript.Utility.Services
{
  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection AddAllServiceInterfaces<TService>(
      this IServiceCollection services,
      bool asSingleton)
      where TService : class
    {
      if (asSingleton)
        services.AddSingleton<TService>();
      else
        services.AddScoped<TService>();

      foreach (var i in typeof(TService).GetInterfaces())
        {
          if (asSingleton)
            services.AddSingleton(i, sp => sp.GetRequiredService<TService>());
          else
            services.AddScoped(i, sp => sp.GetRequiredService<TService>());
        }

      return services;
    }
  }
}
