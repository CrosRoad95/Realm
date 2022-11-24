using SlipeServer.Resources.Base;
using System.Reflection;

namespace Realm.Resources.AFK;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] afk { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.AFK.Lua.afk.lua", Assembly);
}