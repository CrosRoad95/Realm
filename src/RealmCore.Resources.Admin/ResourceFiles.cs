using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.Admin;
internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] DebugDraw { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Admin.Lua.debugDraw.lua", Assembly);
    public static byte[] Admin { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Admin.Lua.admin.lua", Assembly);
    public static byte[] ToolElements { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Admin.Lua.Tools.elements.lua", Assembly);
    public static byte[] ToolSpawnMarkers { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Admin.Lua.Tools.spawnMarkers.lua", Assembly);
}
