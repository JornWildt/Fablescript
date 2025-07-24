using Antlr4.StringTemplate;
using Antlr4.StringTemplate.Misc;
using Microsoft.Extensions.Logging;

namespace Fablescript.Core.Templating
{
  internal class TemplateEngineErrorManager : ITemplateErrorListener
  {
    #region Dependencies

    readonly ILogger Logger;

    #endregion


    public TemplateEngineErrorManager(ILogger logger)
    {
      Logger = logger;
    }

    void ITemplateErrorListener.CompiletimeError(TemplateMessage msg)
    {
      Logger.LogError("Template compilation error: {Message}", msg);
    }

    void ITemplateErrorListener.InternalError(TemplateMessage msg)
    {
      Logger.LogError("Template internal error: {Message}", msg);
    }

    void ITemplateErrorListener.IOError(TemplateMessage msg)
    {
      Logger.LogError("Template IO error: {Message}", msg);
    }

    void ITemplateErrorListener.RuntimeError(TemplateMessage msg)
    {
      Logger.LogError("Template runtime error: {Message}", msg);
    }
  }
}
