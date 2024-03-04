namespace RealmCore.Resources.MapNames;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Render { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.MapNames.Lua.render.lua", Assembly);
}
