using SlipeServer.Resources.Base;
using System.Reflection;

namespace Realm.Resources.AdminTools;
internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] DebugDraw { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.AdminTools.Lua.debugDraw.lua", Assembly);
    public static byte[] AdminTools { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.AdminTools.Lua.adminTools.lua", Assembly);
}
