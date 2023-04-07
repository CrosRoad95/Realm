using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.Overlay;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Utility { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Overlay.Lua.utility.lua", Assembly);
    public static byte[] Hud { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Overlay.Lua.hud.lua", Assembly);
    public static byte[] Notifications { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Overlay.Lua.notifications.lua", Assembly);
    public static byte[] Displays { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Overlay.Lua.displays.lua", Assembly);
}
