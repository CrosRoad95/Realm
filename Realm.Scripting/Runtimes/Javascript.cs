namespace Realm.Scripting.Runtimes;

public class LowercaseSymbolsLoader : CustomAttributeLoader
{
    public override T[] LoadCustomAttributes<T>(ICustomAttributeProvider resource, bool inherit)
    {
        var declaredAttributes = base.LoadCustomAttributes<T>(resource, inherit);
        if (IsProtectedResource(resource))
        {
            return declaredAttributes;
        }
        if (!declaredAttributes.Any() && typeof(T) == typeof(ScriptMemberAttribute) && resource is MemberInfo member)
        {
            return new[] { new ScriptMemberAttribute(member.Name.ToTypescriptName()) } as T[];
        }
        return declaredAttributes;
    }

    private static bool IsProtectedResource(ICustomAttributeProvider resource)
    {
        if (resource is MemberInfo member && member.DeclaringType != null)
        {
            if (member.DeclaringType.Assembly.GetName().Name == "ClearScript.Core")
            {
                return true;
            }
            var typeName = member.DeclaringType.FullName;
            return
                typeName.StartsWith("System.Collections.Generic.IAsyncEnumerator") ||
                typeName.StartsWith("System.Collections.Generic.IEnumerator") ||
                typeName == "System.Collections.IEnumerator" ||
                typeName == "System.IDisposable";
        }
        return false;
    }
}

class CustomDocumentLoader : DocumentLoader
{
    private readonly string _basePath;

    public CustomDocumentLoader(string? basePath = null)
    {
        _basePath = basePath ?? "";
    }

    public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
    {
        var file = await File.ReadAllTextAsync(Path.Join(_basePath, "Server", specifier));
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

    public Javascript(ILogger logger, IWorld world, IEvent @event, Func<string?> basePathFactory)
    {
        HostSettings.CustomAttributeLoader = new LowercaseSymbolsLoader();
        _engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableValueTaskPromiseConversion);
        _typescriptTypesGenerator = new TypescriptTypesGenerator();
        _logger = logger.ForContext<IScripting>();

        _engine.DocumentSettings.Loader = new CustomDocumentLoader(basePathFactory());
        _engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableAllLoading;
        _engine.Script.isAsyncFunc = _engine.Evaluate("const ctor = (async() => {}).constructor; x => x instanceof ctor");
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
        AddHostType(typeof(IElement));

        AddHostType(typeof(IEvent));
        AddHostType(typeof(PlayerJoinedEvent));
        AddHostType(typeof(DiscordStatusChannelUpdateContext));
        AddHostType(typeof(FormSubmitDataEvent));

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