using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.AdminTools;
internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] DebugDraw { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.AdminTools.Lua.debugDraw.lua", Assembly);
    public static byte[] AdminTools { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.AdminTools.Lua.adminTools.lua", Assembly);
    public static byte[] ToolsDebugWorld { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.AdminTools.Lua.Tools.debugWorld.lua", Assembly);
}
