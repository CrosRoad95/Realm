namespace Realm.Scripting.Extensions;

internal static class CollectionsExtensions
{
    public static object ToScriptArray(this Array array)
    {
        return ScriptEngine.Current.Script.Array.from(array);
    }
}
