namespace Realm.Scripting.Extensions;

public static class CollectionsExtensions
{
    public static object ToScriptArray(this Array array)
    {
        return ScriptEngine.Current.Script.Array.from(array);
    }
}
