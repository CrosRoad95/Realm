using Realm.Scripting.Classes.Events;

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

internal class Javascript : IScripting
{
    private readonly V8ScriptEngine _engine;
    private readonly TypescriptTypesGenerator _typescriptTypesGenerator;
    public Javascript(IWorld world, IEvent @event)
    {
        HostSettings.CustomAttributeLoader = new LowercaseSymbolsLoader();
        _engine = new V8ScriptEngine();
        _typescriptTypesGenerator = new TypescriptTypesGenerator();

        AddHostType(typeof(JavaScriptExtensions), "javaScriptExtensions");
        AddHostType(typeof(Vector2));
        AddHostType(typeof(Vector4));
        AddHostType(typeof(Vector3));
        AddHostType(typeof(Matrix4x4));
        AddHostType(typeof(Quaternion));
        AddHostType(typeof(Console));
        AddHostType(typeof(Type));

        AddHostType(typeof(World));
        AddHostType(typeof(Spawn));
        AddHostType(typeof(Player));

        AddHostType(typeof(Event));
        AddHostType(typeof(PlayerJoinedEvent));

        AddHostObject("World", world);
        AddHostObject("Event", @event);
    }

    public string GetTypescriptDefinition()
    {
        return _typescriptTypesGenerator.Build();
    }

    public void AddHostType(Type type, string? customName = null)
    {
        _engine.AddHostType(customName ?? type.Name, type);
        _typescriptTypesGenerator.AddType(type);
    }

    public void AddHostObject(string name, object @object)
    {
        _engine.AddHostObject(name, @object);
    }

    public async Task<object> ExecuteAsync(string code)
    {
        return await _engine.Evaluate(code).ToTask();
    }

    public void Execute(string code)
    {
        try
        {
            _engine.Evaluate(code);
        }
        catch(ScriptEngineException ex)
        {
            Console.WriteLine("Javascript exception: {0}", ex);
        }
    }

}