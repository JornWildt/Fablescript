namespace Fablescript.Core.Templating
{
  public interface ITemplateEngine
  {
    string Render(string template, object data);
    string Render(string template, string groupName, string parameterName, object args);
    string LoadAndRender(string fileName, string templateName, string parameterName, object args);
  }
}
