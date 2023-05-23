using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.CEFBlazorGui;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] CEFBlazorGui { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.CEFBlazorGui.Lua.cefBlazorGui.lua", Assembly);
}