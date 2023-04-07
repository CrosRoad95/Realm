using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.Nametags;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] nametags { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Nametags.Lua.nametags.lua", Assembly);
}