using Antlr4.StringTemplate;
using Antlr4.StringTemplate.Misc;
using Fablescript.Utility.Base.Exceptions;
using Microsoft.Extensions.Logging;

namespace Fablescript.Core.Templating
{
  internal class TemplateEngine : ITemplateEngine
  {
    #region Dependencies

    readonly ILogger Logger;

    #endregion


    public TemplateEngine(ILogger<TemplateEngine> logger)
    {
      Logger = logger;
    }


    string ITemplateEngine.Render(string template, object data)
    {
      var group = new TemplateGroup('$', '$');
      group.ErrorManager = new ErrorManager(new TemplateEngineErrorManager(Logger));

      Template st = new Template(group, template);

      if (data is IEnumerable<KeyValuePair<string, object?>> keyValueMap1)
      {
        foreach (var p in keyValueMap1)
        {
          st.Add(p.Key, p.Value);
        }
      }
      else if (data is IEnumerable<KeyValuePair<string, string>> keyValueMap2)
      {
        foreach (var p in keyValueMap2)
        {
          st.Add(p.Key, p.Value);
        }
      }
      else
      {
        foreach (var p in data.GetType().GetProperties())
        {
          var value = p.GetValue(data);
          st.Add(p.Name, value);
        }
      }

      return st.Render();
    }


    string ITemplateEngine.Render(string template, string groupName, string parameterName, object args)
    {
      var t = LoadTemplateFromString(groupName, template, parameterName, args);

      return t.Render();
    }


    /// <summary>
    /// Load group template from embedded ressource and pass data as argument to template.
    /// </summary>
    /// <param name="templateName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    string ITemplateEngine.LoadAndRender(string fileName, string groupName, string parameterName, object args)
    {
      using (var resource = typeof(TemplateEngine).Assembly.GetManifestResourceStream("CBrain.F2.Expert.Implementation.Templates." + fileName))
      {
        string template = LoadTemplateFromResource(fileName);

        var t = LoadTemplateFromString(groupName, template, parameterName, args);

        return t.Render();
      }
    }


    Template LoadTemplateFromString(string groupName, string template, string parameterName, object args)
    {
      var group = new TemplateGroupString("string", template, '$', '$');
      group.ErrorManager = new ErrorManager(new TemplateEngineErrorManager(Logger));
      group.RegisterRenderer(typeof(string), new StringRenderer());
      group.Load();

      var commonTemplates = LoadCommonTemplateGroup();
      group.ImportTemplates(commonTemplates);

      var t = group.GetInstanceOf(groupName);
      if (t == null)
        throw new ConfigurationException($"Could not locate template group '{groupName}'.");

      t.Add(parameterName, args);

      return t;
    }


    string LoadTemplateFromResource(string fileName)
    {
      using (var resource = typeof(TemplateEngine).Assembly.GetManifestResourceStream("CBrain.F2.Expert.Implementation.Templates." + fileName))
      {
        if (resource != null)
        {
          using (var r = new StreamReader(resource))
          {
            string template = r.ReadToEnd();
            return template;
          }
        }
        else
          return $"Template '{fileName}' not found.";
      }
    }


    private TemplateGroup? _CommonTemplateGroup;

    TemplateGroup? LoadCommonTemplateGroup()
    {
      if (_CommonTemplateGroup == null)
      {
        using (var resource = typeof(TemplateEngine).Assembly.GetManifestResourceStream("CBrain.F2.Expert.Implementation.Templates.Common.stg"))
        {
          if (resource != null)
          {
            using (var r = new StreamReader(resource))
            {
              string template = r.ReadToEnd();
              _CommonTemplateGroup = new TemplateGroupString("Common", template, '$', '$');
              _CommonTemplateGroup.Load();
            }
          }
        }
      }

      return _CommonTemplateGroup;
    }
  }
}
