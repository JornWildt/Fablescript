using Fablescript.Core.Contract;
using Fablescript.Core.Engine;
using Fablescript.Core.Fablescript;
using Fablescript.Core.GameConfiguration;
using Fablescript.Core.LLM;
using Fablescript.Core.OpenAI;
using Fablescript.Core.Prompts;
using Fablescript.Core.Templating;
using Fablescript.Utility.Base.Configuration;
using Fablescript.Utility.Base.UnitOfWork;
using Fablescript.Utility.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

      var fablescriptConfig = configuration.GetVerifiedConfigurationSection<FableScriptConfiguration>("Fablescript");

      services.AddSingleton<IFablescriptParser>(serviceProvider =>
      {
        return new FablescriptParser(
          new FablescriptParser.FileWatchParserConfiguration(fablescriptConfig.FableDirectory, fablescriptConfig.SchemaDirectory),
          serviceProvider.GetRequiredService<ILogger<FablescriptParser>>());
      });

      services.AddSingleton<IUnitOfWorkConfigurator<CoreUnitOfWorkContext>, UnitOfWorkConfigurator<CoreUnitOfWorkContext>>();

      var promptConfig = configuration.GetVerifiedConfigurationSection<PromptProviderConfiguration>("Prompts");

      services.AddSingleton(serviceProvider =>
      {
        return new PromptDefinitionParser(
          new PromptDefinitionParser.FileWatchParserConfiguration(promptConfig.PromptDirectory, null),
          serviceProvider.GetRequiredService<ILogger<PromptDefinitionParser>>());
      });
      services.AddSingleton<IPromptDefinitionProvider, TextFilePromptDefinitionProvider>();

      services.AddSingleton<IPromptRunner, PromptRunner>();
      services.AddSingleton<IStructuredPromptRunner, StructuredPromptRunner>();
      
      services.AddSingleton<ITemplateEngine, TemplateEngine>();

      services.AddVerifiedConfiguration<AIClientConfiguration>(configuration, "AIClient");

      services.AddSingleton<ILLMGenerator, OpenAILLMGenerator>();
      services.AddSingleton<ILLMStructuredGenerator, LLMStructuredGenerator>();

      return services;
    }
  }
}
