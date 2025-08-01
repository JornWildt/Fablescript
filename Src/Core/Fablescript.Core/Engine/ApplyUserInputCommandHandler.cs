using Fablescript.Core.Contract.Engine.Commands;
using Fablescript.Core.Fablescript;
using Fablescript.Core.Prompts;
using Fablescript.Utility.Services.CommandQuery;
using NLua;

namespace Fablescript.Core.Engine
{
  internal class ApplyUserInputCommandHandler :
    CommandHandlerBase,
    ICommandHandler<ApplyUserInputCommand>
  {
    public ApplyUserInputCommandHandler(
      IFablescriptParser fablescriptParser,
      IStandardLibraryParser standardLibraryParser,
      IGameStateRepository gameStateRepository,
      IPromptRunner promptRunner,
      IStructuredPromptRunner structuredPromptRunner,
      FablescriptConfiguration fablescriptConfig,
      DeveloperConfiguration? developerConfig = null)
      : base(
          fablescriptParser,
          standardLibraryParser,
          gameStateRepository,
          promptRunner,
          structuredPromptRunner,
          fablescriptConfig,
          developerConfig)
    {
    }


    async Task ICommandHandler<ApplyUserInputCommand>.InvokeAsync(ApplyUserInputCommand cmd)
    {
      var game = await GameStateRepository.GetAsync(cmd.GameId);
      var locationSrc = (LuaTable)game.Player.location;

      dynamic location = new LuaObject(locationSrc);

      var player = (LuaTable)game.Player.Source;

      var objectsHere = LuaConverter.ConvertLuaTableToEnumerable((LuaTable)location.objects_here)
        .Where(o => !player.Equals(o.Source))
        .ToArray();
      var facts = LuaConverter.ConvertLuaTableToEnumerable((LuaTable)location.facts).ToArray();
      var exits = LuaConverter.ConvertLuaTableToEnumerable((LuaTable)location.exits).ToArray();

      var std = await StandardLibraryParser.GetStandardLibrary();
      var commands = std.Commands;

      // TODO: Command list generation for prompt can be done once and for all (for globally available commands ... without conditions)

      // FIXME: extract list of parameters such as "{object}" or "{exit}" and use it to generate the JSON intent template
      // - Afterwards, use it to match parameters on the commands

      // FIXME: Can be precalculated
      var parameterNames = commands.Values
        .SelectMany(c => c.Parameters)
        .Select(c => c.Name)
        .Distinct()
        .ToList();

      var args = new
      {
        Location = (string)location.title,
        Facts = facts.Select(f => f.text).ToArray() ?? [],
        HasFacts = facts.Length > 0,
        Exits = exits.Select(x => new { Name = x.name, Title = x.title, Description = x.description }).ToArray(),
        HasExits = exits.Length > 0,
        ObjectsHere = objectsHere.Select(o => new { Name = (string)o.name, Title = (string)o.title, Description = (string?)o.description }).ToArray(),
        HasObjectsHere = objectsHere.Length > 0,
        Commands = commands.Values.Select(c => new { c.Name, c.Intention, c.Usage }).ToArray(),
        cmd.PlayerInput,
        ParameterNames = parameterNames
      };
      var response = await StructuredPromptRunner.RunPromptAsync<Dictionary<string, string>>("DecodeUserIntent", args);

      var decoded = response.Select(i => i.Key + "=" + i.Value).Aggregate("", (a, b) => a + ", " + b);
      cmd.Answer.Value = "Intent: " + decoded;

      if (response.TryGetValue("intent", out string? intent))
      {
        if (intent != "other")
        {
          if (commands.TryGetValue(intent, out var command))
          {
            var parameters = new List<object?>();
            foreach (var p in command.Parameters)
            {
              string? value = null;
              response.TryGetValue(p.Name, out value);
              parameters.Add(value);
            }

            game.InvokeFunction(command.Invoke, parameters.ToArray());
            cmd.Answer.Value = string.Join("\n", game.ResponseOutput);
            return;
          }
        }
      }

      var idleResponse = await PromptRunner.RunPromptAsync("IdleUserInputResponse", args);
      cmd.Answer.Value += idleResponse;

#if false
      if (response.intent == "move" && response.move_exit_name != null)
      {
        var exit = exits.FirstOrDefault(x => x.Name == response.move_exit_name);
        if (exit != null)
        {
          var newLocationId = exit.TargetLocationId;
          LuaTable newLocation = game.GetObject(newLocationId);
          if (newLocation != null)
          {
            game.Player.LocationId = newLocationId;

            var sceneDescription = await DescribeScene(game, newLocation);
            cmd.Answer.Value = sceneDescription;
            return;
          }
        }
      }

#endif
    }
  }
}
