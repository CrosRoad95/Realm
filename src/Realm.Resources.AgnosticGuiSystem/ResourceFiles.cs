using SlipeServer.Resources.Base;
using System.Reflection;

namespace Realm.Resources.AgnosticGuiSystem;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Controller { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.AgnosticGuiSystem.Lua.controller.lua", Assembly);
    public static byte[] Utilities { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.AgnosticGuiSystem.Lua.utilities.lua", Assembly);
    public static byte[] CeGuiProvider { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.AgnosticGuiSystem.Lua.cegui.lua", Assembly);
}
