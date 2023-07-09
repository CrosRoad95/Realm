using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.Addons.GuiSystem.DGS;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Provider { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Addons.GuiSystem.DGS.Lua.provider.lua", Assembly);
}
