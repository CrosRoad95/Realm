using SlipeServer.Resources.Base;
using System.Reflection;
namespace Realm.Resources.LuaInterop;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Debugging { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.LuaInterop.Lua.debugging.lua", Assembly);
}
