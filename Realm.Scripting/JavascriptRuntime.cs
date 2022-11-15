namespace Realm.Scripting;

internal class JavascriptRuntime : IScriptingModuleInterface, IReloadable
{
    private readonly V8ScriptEngine _engine;
    private readonly TypescriptTypesGenerator _typescriptTypesGenerator;
    private readonly ILogger _logger;
    private readonly Func<string?> _basePathFactory;

    public JavascriptRuntime(ILogger logger, Func<string?> basePathFactory, EventScriptingFunctions eventFunctions, ModulesScriptingFunctions modulesFunctions, UlitityScriptingFunctions ulitityFunctions)
    {
        HostSettings.CustomAttributeLoader = new LowercaseSymbolsLoader();
        _engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion);
        _typescriptTypesGenerator = new TypescriptTypesGenerator();
        _logger = logger.ForContext<JavascriptRuntime>();

        _engine.DocumentSettings.Loader = new CustomDocumentLoader(basePathFactory());
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

        AddHostObject("Logger", _logger.ForContext("javascript", true), false);
        AddHostObject("Events", eventFunctions, true);
        AddHostObject("Modules", modulesFunctions, true);
        AddHostObject("Utility", ulitityFunctions, true);
        _basePathFactory = basePathFactory;
    }

    public string GetTypescriptDefinition()
    {
        return _typescriptTypesGenerator.Build();
    }

    public void AddHostType(Type type, bool exposeGlobalMembers = false)
    {
        if (exposeGlobalMembers)
            _engine.AddHostType(HostItemFlags.GlobalMembers, type);
        else
            _engine.AddHostType(type.Name, type);
        _typescriptTypesGenerator.AddType(type);
    }

    public void AddHostObject(string name, object @object, bool exposeGlobalMembers = false)
    {
        if(exposeGlobalMembers)
            _engine.AddHostObject(name, HostItemFlags.GlobalMembers, @object);
        else
            _engine.AddHostObject(name, @object);
    }

    public async Task<object> ExecuteAsync(string code, string name)
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
        catch (ScriptEngineException ex)
        {
            _logger.Error(ex, "Failed to execute");
        }

        return null;
    }

    public async Task Start()
    {
        const string fileName = "Server/startup.js";
        _logger.Information("Initializing {fileName}", fileName);
        var fullFileName = Path.Join(_basePathFactory(), fileName);
        var code = File.ReadAllText(fullFileName);
        await Execute(code, fileName);
    }

    public async Task Reload()
    {
        _engine.CollectGarbage(true);
        await Start();
    }

    public int GetPriority() => 50;
}