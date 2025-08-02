using NLua;

namespace Fablescript.Core.Engine
{
  internal interface IBuiltInLuaFunctions
  {
    string run_prompt(string promptName, LuaTable luaArgs);
  }
}
