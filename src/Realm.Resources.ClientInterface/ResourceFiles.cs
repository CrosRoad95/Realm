using SlipeServer.Resources.Base;
using System.Reflection;
namespace Realm.Resources.ClientInterface;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Debugging { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.ClientInterface.Lua.debugging.lua", Assembly);
    public static byte[] Utility { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.ClientInterface.Lua.utility.lua", Assembly);
    public static byte[] Player { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.ClientInterface.Lua.player.lua", Assembly);
    public static byte[] Focusable { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.ClientInterface.Lua.focusable.lua", Assembly);
}
