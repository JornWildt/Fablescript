using Fablescript.Utility.Base.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fablescript.Utility.Base.Configuration
{
  public static class ConfigurationExtensions
  {
    public static IServiceCollection AddVerifiedConfiguration<T>(
      this IServiceCollection services,
      IConfiguration config,
      string sectionName,
      Action<T>? configurationModifier = null)
      where T : class, new()
    {
      var result = GetVerifiedConfigurationSection<T>(config, sectionName);
      if (configurationModifier != null)
      {
        configurationModifier(result);
      }
      services.AddSingleton(result);
      return services;
    }

    
    public static bool TryGetVerifiedConfigurationSection<T>(this IConfiguration config, string sectionName, out T result)
      where T : class, new()
    {
      var tmp = GetInternalVerifiedConfigurationSection<T>(config, sectionName, isOptional: true);
      if (tmp != null)
      {
        result = tmp;
        return true;
      }
      else
      {
        result = new T();
        return false;
      }
    }


    public static T GetVerifiedConfigurationSection<T>(this IConfiguration config, string sectionName)
      where T : class, new()
    {
      return GetInternalVerifiedConfigurationSection<T>(config, sectionName, isOptional: false)!;
    }


    static T? GetInternalVerifiedConfigurationSection<T>(IConfiguration config, string sectionName, bool isOptional)
      where T : class, new()
    {
      IConfigurationSection? section = null;

      try
      {
        section = config.GetRequiredSection(sectionName);
      }
      catch (Exception)
      {
        if (isOptional)
          return null;
        else
          throw new ConfigurationException($"No configuration section '{sectionName}' found.");
      }

      var cfg = new T();
      section.Bind(cfg);

      MergeVariables(config, cfg);
      ObjectValidator.ValidateObject($"In '{sectionName}' configuration: ", cfg);

      return cfg;
    }


    static public void MergeVariables<T>(IConfiguration config, T cfg)
    {
      var variables = GetConfigurationVariables(config);
      // FIXME var settings = new TextMergeSettings();

      var cfgType = typeof(T);
      foreach (var property in cfgType.GetProperties())
      {
        if (property.PropertyType == typeof(string))
        {
          var attributes = property.GetCustomAttributes(typeof(MergeableAppSettingAttribute), false);
          if (attributes.Any(a => a is MergeableAppSettingAttribute))
          {
            string? value = (string?)property.GetValue(cfg, null);
            if (value != null)
            {
              var mergedValue = value; // FIXME  TextMerge.Merge(value, variables, settings);
              property.SetValue(cfg, mergedValue, null);
            }
          }
        }
      }
    }


    static Dictionary<string, object>? _ConfigurationVariables = null;


    static public Dictionary<string, object> GetConfigurationVariables(IConfiguration config)
    {
      if (_ConfigurationVariables == null)
      {
        const string sectionName = "ConfigurationVariables";
        _ConfigurationVariables = new Dictionary<string, object>();
        var section = config.GetSection(sectionName);
        if (section != null)
        {
          foreach (var item in section.AsEnumerable())
          {
            if (item.Key.StartsWith(sectionName + ":"))
            {
              _ConfigurationVariables.Add(item.Key.Substring(sectionName.Length + 1), item.Value ?? "");
            }
          }
        }
      }

      return _ConfigurationVariables;
    }
  }
}
