using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Extensions.Logging;

namespace Fablescript.Utility.Base
{
  public abstract class FileWatchParser<ParseType, CacheType>
    where ParseType : class
    where CacheType : class
  {
    #region Dependencies

    protected readonly FileWatchParserConfiguration Configuration;
    protected readonly ILogger<FileWatchParser<ParseType, CacheType>> Logger;

    #endregion


    public class ParserException : Exception
    {
      public ParserException(string msg)
        : base(msg) { }
    }


    public record FileWatchParserConfiguration(string SourceDir , string? SchemaDir);


    #region Configuration settings


    //private static string? _sourceDir;
    //public static string SourceDir
    //{
    //  get
    //  {
    //    if (_sourceDir == null)
    //    {
    //      _sourceDir = FileUtils.MapPathToBaseDir(SourceDirAppSetting, defaultValue: DefaultSourceDir);
    //    }
    //    return _sourceDir;
    //  }
    //  set
    //  {
    //    string mappedPath = FileUtils.MapPathToBaseDir(value);
    //    if (_sourceDir != mappedPath)
    //    {
    //      ClearCache();
    //      _sourceDir = mappedPath;
    //      TryCreateOrSetFileWatcherPath();
    //    }
    //  }
    //}


    //private static string? _schemaDir;
    //public static string SchemaDir
    //{
    //  get
    //  {
    //    if (_schemaDir == null)
    //    {
    //      _schemaDir = FileUtils.MapPathToBaseDir(SchemaDirAppSetting, defaultValue: DefaultSchemaDir);
    //    }
    //    return _schemaDir;
    //  }
    //  set
    //  {
    //    string mappedPath = FileUtils.MapPathToBaseDir(value);
    //    if (_schemaDir != mappedPath)
    //    {
    //      ClearCache();
    //      _schemaDir = mappedPath;
    //      TryCreateOrSetFileWatcherPath();
    //    }
    //  }
    //}

    #endregion


    protected FileSystemWatcher Watcher;
    protected List<string> ParseErrors = new List<string>();

    protected object ParseLocker = new object();


    public event EventHandler? CacheClearedEvent;

    private CacheType? _Cache;
    private CacheType? Cache
    {
      get => _Cache;
      set
      {
        if (value == null)
          CacheClearedEvent?.Invoke(null, new EventArgs());
        _Cache = value;
      }
    }


    public event EventHandler? FilesParsedEvent;


    protected ValidationEventHandler OnValidationError = (s, arg) => { throw arg.Exception; };

    protected virtual string NotInCacheErrorMsg
    {
      get { return "Could not find key '{0}' in cache"; }
    }


    public FileWatchParser(
      FileWatchParserConfiguration configuration,
      ILogger<FileWatchParser<ParseType, CacheType>> logger)
    {
      Configuration = configuration;
      Logger = logger;

      Watcher = new FileSystemWatcher();
      Watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
      Watcher.Changed += OnChanged;
      Watcher.Created += OnChanged;
      Watcher.Deleted += OnChanged;
      Watcher.Renamed += OnRenamed;
      Watcher.Filter = "*.*";
    }


    private void TryCreateOrSetFileWatcherPath()
    {
      if (Configuration == null || Configuration.SourceDir == null)
      {
        Watcher.EnableRaisingEvents = false;
      }
      else
      {
        try
        {
          if (Watcher.Path != Configuration.SourceDir || !Watcher.EnableRaisingEvents)
          {
            if (!Directory.Exists(Configuration.SourceDir))
            {
              Directory.CreateDirectory(Configuration.SourceDir);
            }
            Logger.LogDebug("Assigning path '{SourceDir}' to FileWatchParser {ParserType}.", Configuration.SourceDir, typeof(FileWatchParser<ParseType, CacheType>));
            Watcher.Path = Configuration.SourceDir;
            Watcher.EnableRaisingEvents = true;
          }
        }
        catch (Exception ex)
        {
          Watcher.EnableRaisingEvents = false;
          Logger.LogError(ex, "Could not create directory: {SourceDir} or set it as FileSystemWatcher path", Configuration.SourceDir);
        }
      }
    }


    void OnRenamed(object sender, RenamedEventArgs e)
    {
      Cache = null;
    }


    void OnChanged(object sender, FileSystemEventArgs e)
    {
      Logger.LogDebug("File change detected for '{Name}'. Clearing file watcher cache.", e.Name);
      ClearCache();
    }


    public CacheType GetCache()
    {
      lock (ParseLocker)
      {
        CacheType? localCache = Cache;

        if (localCache == null)
        {
          localCache = Cache = Parse();
          OnFilesParsed();
        }

        if (ParseErrors != null && ParseErrors.Count > 0)
        {
          StringBuilder errors = new StringBuilder();
          foreach (string error in ParseErrors)
          {
            errors.AppendLine(error);
          }
          throw new ParserException(errors.ToString());
        }

        return localCache;
      }
    }


    public void ClearCache()
    {
      lock (ParseLocker)
      {
        Cache = null;
      }
    }


    protected virtual void OnFilesParsed()
    {
      FilesParsedEvent?.Invoke(this, new EventArgs());
    }


    protected abstract CacheType Parse();


    protected T ExecuteWithErrorHandling<T>(string filename, Func<T> f)
    {
      try
      {
        return f();
      }
      catch (InvalidOperationException ex)
      {
        string msg = ex.Message;
        Exception? innerEx = ex.InnerException;
        while (innerEx != null)
        {
          msg += " (" + innerEx.Message + ")";
          innerEx = innerEx.InnerException;
        }
        msg = string.Format("Reading file '{0}' failed: {1}.", filename, msg);
        throw new ParserException(msg);
      }
    }


    protected XmlReaderSettings CreateXmlReaderSettings(string schemaName)
    {
      XmlReaderSettings settings = new XmlReaderSettings();
      if (Configuration.SchemaDir != null && schemaName != null)
      {
        settings.ValidationType = ValidationType.Schema;
        try
        {
          settings.Schemas.Add(ParseSchema(Configuration.SchemaDir, schemaName, OnValidationError));
        }
        catch (XmlSchemaException e) { throw new ParserException(e.SourceUri + " is invalid. Items will not be loaded."); }
        catch (FileNotFoundException e) { throw new ParserException(e.Message); }
        settings.ValidationEventHandler += OnValidationError;
      }
      return settings;
    }


    protected XmlSchema ParseSchema(string schemaDir, string schemaName, ValidationEventHandler onValidationError)
    {
      using (XmlTextReader r = new XmlTextReader(Path.Combine(schemaDir, schemaName)))
      {
        return XmlSchema.Read(r, onValidationError) ?? throw new ParserException("XmlSchema.Read() returned null.");
      }
    }
  }
}
