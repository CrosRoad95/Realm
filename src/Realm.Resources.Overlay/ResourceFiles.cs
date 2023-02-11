using SlipeServer.Resources.Base;
using System.Reflection;

namespace Realm.Resources.Overlay;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Utility { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.Overlay.Lua.utility.lua", Assembly);
    public static byte[] Hud { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.Overlay.Lua.hud.lua", Assembly);
    public static byte[] Notifications { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.Overlay.Lua.notifications.lua", Assembly);
}
