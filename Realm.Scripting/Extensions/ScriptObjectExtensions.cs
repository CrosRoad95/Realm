namespace Realm.Scripting.Extensions;

internal static class ScriptObjectExtensions
{
    public static bool Is(this ScriptObject func1, ScriptObject func2) => func1.Engine.Equals(func2.Engine) && func1.Engine.Script.Object.@is(func1, func2);
}
