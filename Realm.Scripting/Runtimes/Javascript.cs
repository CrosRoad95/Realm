using Realm.Scripting.Classes;

namespace Realm.Scripting.Runtimes;

internal class Javascript : IScripting
{
    private readonly V8ScriptEngine _engine;
    public Javascript(IWorld world)
    {
        // 1443589824
        _engine = new V8ScriptEngine();
        AddHostType("JavaScriptExtensions", typeof(JavaScriptExtensions));
        AddHostType("Vector3", typeof(Vector3));
        AddHostType("Console", typeof(Console));
        AddHostType("World", typeof(World));

        AddHostObject("World", world);
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