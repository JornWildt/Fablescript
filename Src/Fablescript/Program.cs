using System.Diagnostics;
using Fablescript.Core;
using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Database;
using Fablescript.Core.GameConfiguration;
using Fablescript.Utility.Services.CommandQuery;
using Fablescript.Utility.Services.Contract.CommandQuery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fablescript
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

      builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

      ActivitySource programSource = new ActivitySource(builder.Environment.ApplicationName, "1.0.0");
      builder.Services.AddSingleton(programSource);

      builder.AddServiceDefaults();

      builder.Services
        .AddCore(builder.Configuration)
        .AddCorePersistence(builder.Configuration)
        .AddMain(builder.Configuration);

      builder.Services.AddSingleton<ConsoleRunner>();

      using IHost host = builder.Build();

      var runner = host.Services.GetRequiredService<ConsoleRunner>();
      await runner.Run();
    }


    class ConsoleRunner
    {
      #region Dependencies

      readonly ICommandProcessor CommandProcessor;

      #endregion


      public ConsoleRunner(
        ICommandProcessor commandProcessor)
      {
        CommandProcessor = commandProcessor;
      }


      public async Task Run()
      {
        var initCommand = new DescribeSceneCommand(TemporaryConstants.PlayerId, new CommandOutput<string>());
        await CommandProcessor.InvokeCommandAsync(initCommand);

        Console.WriteLine(initCommand.Answer.Value);

        do
        {
          var input = Console.ReadLine();
          if (input != null)
          {
            var applyCmd = new ApplyUserInputCommand(TemporaryConstants.PlayerId, input, new CommandOutput<string>());
            await CommandProcessor.InvokeCommandAsync(applyCmd);

            Console.WriteLine(applyCmd.Answer.Value);
          }
        }
        while (true);
      }
    }
  }
}
