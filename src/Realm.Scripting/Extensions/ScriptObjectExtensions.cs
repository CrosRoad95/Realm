namespace Realm.Module.Scripting.Extensions;

public static class ScriptObjectExtensions
{
    public static bool Is(this ScriptObject func1, ScriptObject func2) => func1.Engine.Equals(func2.Engine) && func1.Engine.Script.Object.@is(func1, func2);
    public static bool IsAsync(this ScriptObject func) => func.Engine.Script.isAsyncFunc(func);
    public static bool IsAsync(this object func, ScriptEngine engine) => engine.Script.isAsyncFunc(func);

    public static object[]? ConvertArray(this ScriptObject? arg)
    {
        if (arg == null)
            return null;

        dynamic scriptObject = arg;
        if (scriptObject.constructor.name == "Array")
        {
            int length = Convert.ToInt32(scriptObject.length);
            var array = new object[length];
            for (var index = 0; index < length; ++index)
            {
                array[index] = scriptObject[index];
            }
            return array;
        }
        return null;
    }
}
