namespace Realm.Scripting.Runtimes;

class LowercaseSymbolsLoader : CustomAttributeLoader
{
    public override T[]? LoadCustomAttributes<T>(ICustomAttributeProvider resource, bool inherit)
    {
        var declaredAttributes = base.LoadCustomAttributes<T>(resource, inherit);
        if (!declaredAttributes.Any() && typeof(T) == typeof(ScriptMemberAttribute) && resource is MemberInfo member)
        {
            return new[] { new ScriptMemberAttribute(member.Name.ToTypescriptName()) } as T[];
        }
        return declaredAttributes;
    }
}

class CustomDocumentLoader : DocumentLoader
{
    public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
    {
        var path = Path.GetDirectoryName(
              System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)[6..];
        var file = await File.ReadAllTextAsync(Path.Join(path, "Server", specifier));
        return new StringDocument(new DocumentInfo(specifier)
        {
            Category = category,
            ContextCallback = contextCallback
        }, file);
    }
}

internal class Javascript : IScripting
{
    private readonly V8ScriptEngine _engine;
    private readonly TypescriptTypesGenerator _typescriptTypesGenerator;
    private readonly ILogger _logger;

    public Javascript(ILogger logger, IWorld world, IEvent @event)
    {
        HostSettings.CustomAttributeLoader = new LowercaseSymbolsLoader();
        _engine = new V8ScriptEngine();
        _typescriptTypesGenerator = new TypescriptTypesGenerator();
        _logger = logger.ForContext<IScripting>();

        _engine.DocumentSettings.Loader = new CustomDocumentLoader();
        _engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableAllLoading;

        AddHostType(typeof(JavaScriptExtensions));
        AddHostType(typeof(Vector2));
        AddHostType(typeof(Vector4));
        AddHostType(typeof(Vector3));
        AddHostType(typeof(Matrix4x4));
        AddHostType(typeof(Quaternion));
        AddHostType(typeof(Console));
        AddHostType(typeof(Type));

        AddHostType(typeof(IRPGPlayer));
        AddHostType(typeof(IWorld));
        AddHostType(typeof(ISpawn));

        AddHostType(typeof(IEvent));
        AddHostType(typeof(PlayerJoinedEvent));

        AddHostObject("World", world);
        AddHostObject("Event", @event);
    }

    public string GetTypescriptDefinition()
    {
        return _typescriptTypesGenerator.Build();
    }

    public void AddHostType(Type type)
    {
        _engine.AddHostType(type.Name, type);
        _typescriptTypesGenerator.AddType(type);
    }

    public void AddHostObject(string name, object @object)
    {
        _engine.AddHostObject(name, @object);
    }

    public async Task<object> ExecuteAsync(string code, string name)
    {
        var documentInfo = new DocumentInfo(name)
        {
            Category = ModuleCategory.Standard
        };
        return await _engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, code).ToTask();
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
        catch(ScriptEngineException ex)
        {
            _logger.Error(ex, "Failed to execute");
        }
    }

}