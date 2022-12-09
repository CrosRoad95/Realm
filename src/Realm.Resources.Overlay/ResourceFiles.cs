using SlipeServer.Resources.Base;
using System.Reflection;

namespace Realm.Resources.Overlay;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Overlay { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.Overlay.Lua.overlay.lua", Assembly);
    public static byte[] Notifications { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.Overlay.Lua.notifications.lua", Assembly);
}
