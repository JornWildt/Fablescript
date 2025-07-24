using Fablescript.Utility.Base.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Fablescript.Utility.Base
{
  public static class ServiceDiscoveryUtility
  {
    public static string GetServiceUrl(
      this IConfiguration configuration,
      string serviceName)
    {
      return GetServiceUrl(configuration, serviceName, ["https", "http"]);
    }
    
    
    public static string GetServiceUrl(
      this IConfiguration configuration,
      string serviceName,
      IReadOnlyCollection<string> endpoints)
    {
      foreach (var endpoint in endpoints)
      {
        var envName = $"Services:{serviceName}:{endpoint}:0";
        var url = configuration.GetValue<string>(envName);
        if (url != null)
          return url;
      }
      throw new ConfigurationException($"No connection string 'Services:{serviceName}:{endpoints.FirstOrDefault() ?? "-none-"}:0' supplied.");
    }
  }
}
