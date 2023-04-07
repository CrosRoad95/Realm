using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.AFK;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] afk { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.AFK.Lua.afk.lua", Assembly);
}