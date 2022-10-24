namespace Realm.Scripting;

internal class JavascriptRuntime : IScriptingModuleInterface
{
    private readonly V8ScriptEngine _engine;
    private readonly TypescriptTypesGenerator _typescriptTypesGenerator;
    private readonly ILogger _logger;

    public JavascriptRuntime(ILogger logger, Func<string?> basePathFactory, EventFunctions eventFunctions, ModulesFunctions modulesFunctions)
    {
        HostSettings.CustomAttributeLoader = new LowercaseSymbolsLoader();
        _engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion);
        _typescriptTypesGenerator = new TypescriptTypesGenerator();
        _logger = logger.ForContext<JavascriptRuntime>();

        _engine.DocumentSettings.Loader = new CustomDocumentLoader(basePathFactory());
        _engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableAllLoading;
        _engine.Script.isAsyncFunc = _engine.Evaluate("const ctor = (async() => {}).constructor; x => x instanceof ctor");
        AddHostType(typeof(JavaScriptExtensions));
        AddHostType(typeof(Vector4));
        AddHostType(typeof(Vector2));
        AddHostType(typeof(Vector3));
        AddHostType(typeof(Matrix4x4));
        AddHostType(typeof(Quaternion));
        AddHostType(typeof(Type));

        AddHostObject("Logger", _logger.ForContext("javascript", true), false);
        AddHostObject("Events", eventFunctions, true);
        AddHostObject("Modules", modulesFunctions, true);
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

    public void Execute(string code, string name)
    {
        try
        {
            var documentInfo = new DocumentInfo(name)
            {
                Category = ModuleCategory.Standard
            };
            _engine.Evaluate(documentInfo, code);
        }
        catch (ScriptEngineException ex)
        {
            _logger.Error(ex, "Failed to execute");
        }
    }

}