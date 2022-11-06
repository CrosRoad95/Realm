using SlipeServer.Resources.Base;
using System.Reflection;

namespace Realm.Resources.Addons.AgnosticGuiSystem.CeGuiProvider;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Provider { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.Addons.AgnosticGuiSystem.CeGuiProvider.Lua.provider.lua", Assembly);
}
