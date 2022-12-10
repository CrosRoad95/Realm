using Realm.Interfaces.Providers;
using Realm.Module.Scripting.Extensions;
using Realm.Module.Scripting.Functions;
using Realm.Module.Scripting.Interfaces;

namespace Realm.Module.Scripting;

internal class JavascriptRuntime : IScriptingModuleInterface, IReloadable
{
    private readonly V8ScriptEngine _engine;
    private readonly ILogger _logger;
    private readonly IServerFilesProvider _serverFilesProvider;

    public event Action<Type, bool>? HostTypeAdded;

    public JavascriptRuntime(ILogger logger, EventScriptingFunctions eventFunctions, ModulesScriptingFunctions modulesFunctions,
        UlitityScriptingFunctions ulitityFunctions, IServerFilesProvider serverFilesProvider)
    {
        HostSettings.CustomAttributeLoader = new LowercaseSymbolsLoader();
        _engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion);
        _logger = logger.ForContext<JavascriptRuntime>();

        _engine.AllowReflection = true;
        _engine.DocumentSettings.Loader = new CustomDocumentLoader(serverFilesProvider);
        _engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableAllLoading;
        _engine.Script.isAsyncFunc = _engine.Evaluate("const ctor = (async() => {}).constructor; x => x instanceof ctor");
        AddHostType(typeof(JavaScriptExtensions));
        AddHostType(typeof(Vector2));
        AddHostType(typeof(Vector3));
        AddHostType(typeof(Vector4));
        AddHostType(typeof(Color));
        AddHostType(typeof(Matrix4x4));
        AddHostType(typeof(Quaternion));
        AddHostType(typeof(Type));

        AddHostObject("host", new HostFunctions());
        AddHostObject("Logger", new LoggerScriptingFunctions(_logger));
        AddHostObject("Events", eventFunctions, true);
        AddHostObject("Modules", modulesFunctions, true);
        AddHostObject("Utility", ulitityFunctions, true);
        _serverFilesProvider = serverFilesProvider;
    }

    public void AddHostType(Type type, bool exposeGlobalMembers = false)
    {
        if (exposeGlobalMembers)
            _engine.AddHostType(HostItemFlags.GlobalMembers, type);
        else
            _engine.AddHostType(type.Name, type);
        HostTypeAdded?.Invoke(type, exposeGlobalMembers);
    }

    public void AddHostObject(string name, object @object, bool exposeGlobalMembers = false)
    {
        if (exposeGlobalMembers)
            _engine.AddHostObject(name, HostItemFlags.GlobalMembers, @object);
        else
            _engine.AddHostObject(name, @object);
    }

    public async Task<object?> ExecuteAsync(string code, string name)
    {
        var documentInfo = new DocumentInfo(name)
        {
            Category = ModuleCategory.Standard
        };
        return await _engine.Evaluate(documentInfo, code).ToTask();
    }

    public async Task<object?> Execute(string code, string name)
    {
        try
        {
            var documentInfo = new DocumentInfo(name)
            {
                Category = ModuleCategory.Standard
            };
            var result = _engine.Evaluate(documentInfo, code);
            if (result.IsAsync(_engine))
                return await result.ToTask();
            return result;

        }
        catch (ScriptEngineException scriptEngineException)
        {
            var scriptException = scriptEngineException as IScriptEngineException;
            if (scriptException != null)
            {
                _logger.Error("Exception thrown while executing script: {errorDetails}", scriptException.ErrorDetails);
            }
            else
                _logger.Error("Exception thrown while executing event");
        }

        return null;
    }

    public async Task Start()
    {
        const string fileName = "Scripts/startup.js";
        _logger.Information("Initializing {fileName}", fileName);
        try
        {
            var code = await _serverFilesProvider.ReadAllText(fileName);
            await Execute(code, fileName);
        }
        catch(Exception)
        {
            _logger.Information("Failed to initialize {fileName}", fileName);
            throw;
        }
    }

    public async Task Reload()
    {
        _engine.CollectGarbage(true);
        await Start();
    }

    public int GetPriority() => 50;
}