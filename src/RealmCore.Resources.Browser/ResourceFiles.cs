namespace RealmCore.Resources.Browser;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Browser { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Browser.Lua.Browser.lua", Assembly);
    public static byte[] ErrorPage { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Browser.Lua.error.html", Assembly);
}