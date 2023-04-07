using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.Addons.AgnosticGuiSystem.DGSProvider;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Provider { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Addons.AgnosticGuiSystem.DGSProvider.Lua.provider.lua", Assembly);
}
