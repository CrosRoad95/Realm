namespace Realm.Module.Scripting.Extensions;

public static class CollectionsExtensions
{
    public static object ToScriptArray(this Array array)
    {
        return ScriptEngine.Current.Script.Array.from(array);
    }

    public static object ToScriptArray(this Array array, ScriptEngine scriptEngine)
    {
        return scriptEngine.Script.Array.from(array);
    }
}
