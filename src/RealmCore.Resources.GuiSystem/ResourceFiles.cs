using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.GuiSystem;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Controller { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.GuiSystem.Lua.controller.lua", Assembly);
    public static byte[] Utilities { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.GuiSystem.Lua.utilities.lua", Assembly);
    public static byte[] CeGuiProvider { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.GuiSystem.Lua.cegui.lua", Assembly);
}
