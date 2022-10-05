using Realm.Interfaces.Scripting.Classes;
using Realm.Scripting.Classes;
using System.Reflection;

namespace Realm.Scripting.Runtimes;

class LowercaseSymbolsLoader : CustomAttributeLoader
{
    public override T[]? LoadCustomAttributes<T>(ICustomAttributeProvider resource, bool inherit)
    {
        var declaredAttributes = base.LoadCustomAttributes<T>(resource, inherit);
        if (!declaredAttributes.Any() && typeof(T) == typeof(ScriptMemberAttribute) && resource is MemberInfo member)
        {
            var lowerCamelCaseName = char.ToLowerInvariant(member.Name[0]) + member.Name[1..];
            return new[] { new ScriptMemberAttribute(lowerCamelCaseName) } as T[];
        }
        return declaredAttributes;
    }
}

internal class Javascript : IScripting
{
    private readonly V8ScriptEngine _engine;
    public Javascript(IWorld world)
    {
        HostSettings.CustomAttributeLoader = new LowercaseSymbolsLoader();
        _engine = new V8ScriptEngine();
        AddHostType("javaScriptExtensions", typeof(JavaScriptExtensions));
        AddHostType("vector3", typeof(Vector3));
        AddHostType("console", typeof(Console));

        AddHostType("world", typeof(World));
        AddHostType("spawn", typeof(Spawn));

        AddHostObject("world", world);
    }

    public void AddHostType(string name, Type type)
    {
        _engine.AddHostType(name, type);
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
        _engine.Evaluate(code);
    }

}