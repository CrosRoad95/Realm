namespace Realm.Scripting.Runtimes;

public class Test
{
    public static async Task<double> Add(double a, double b)
    {
        await Task.Delay(1000);
        return a + b;
    }
}

internal class Javascript : IScripting
{
    private readonly V8ScriptEngine _engine;
    public Javascript()
    {
        _engine = new V8ScriptEngine();
        AddType(typeof(JavaScriptExtensions));
        AddType(typeof(Test));
        AddType(typeof(Console));
    }

    public void AddType(Type type)
    {
        _engine.AddHostType(type.Name, type);
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