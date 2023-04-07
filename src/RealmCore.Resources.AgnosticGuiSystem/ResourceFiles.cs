using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.AgnosticGuiSystem;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Controller { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.AgnosticGuiSystem.Lua.controller.lua", Assembly);
    public static byte[] Utilities { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.AgnosticGuiSystem.Lua.utilities.lua", Assembly);
    public static byte[] CeGuiProvider { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.AgnosticGuiSystem.Lua.cegui.lua", Assembly);
}
