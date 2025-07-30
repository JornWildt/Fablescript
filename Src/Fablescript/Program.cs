using System.Diagnostics;
using Fablescript.Core;
using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Core.Database;
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
        var startCmd = new StartGameCommand(
          new FableId("Jokull"),
          new CommandOutput<Core.Contract.Engine.GameId>());

        await CommandProcessor.InvokeCommandAsync(startCmd);
        var gameId = startCmd.CreatedGameId.Value!;

        var describeCmd = new DescribeSceneCommand(gameId, new CommandOutput<string>());
        await CommandProcessor.InvokeCommandAsync(describeCmd);

        Console.WriteLine(describeCmd.Answer.Value);

        do
        {
          var input = Console.ReadLine();
          if (input != null)
          {
            var applyCmd = new ApplyUserInputCommand(gameId, input, new CommandOutput<string>());
            await CommandProcessor.InvokeCommandAsync(applyCmd);

            Console.WriteLine(applyCmd.Answer.Value);
          }
        }
        while (true);
      }
    }
  }
}
