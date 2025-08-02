using Fablescript.Core.Prompts;
using NLua;

namespace Fablescript.Core.Engine
{
  internal class BuiltInLuaFunctions : IBuiltInLuaFunctions
  {
    #region Dependencies

    private readonly IPromptRunner PromptRunner;

    #endregion


    public BuiltInLuaFunctions(
      IPromptRunner promptRunner)
    {
      PromptRunner = promptRunner;
    }


    string IBuiltInLuaFunctions.run_prompt(string promptName, LuaTable luaArgs)
    {
      return RunAsync(() => run_prompt(promptName, luaArgs));
    }


    private async Task<string> run_prompt(string promptName, LuaTable luaArgs)
    {
      var args = LuaConverter.ConvertLuaTableToDictionaryOrList(luaArgs);
      var response = await PromptRunner.RunPromptAsync("DescribeScene", args);
      return response;
    }


    protected T RunAsync<T>(Func<Task<T>> f)
    {
      return Task.Run(async () => await f()).GetAwaiter().GetResult();
    }
  }
}
