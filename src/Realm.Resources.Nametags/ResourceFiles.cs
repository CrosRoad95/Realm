using SlipeServer.Resources.Base;
using System.Reflection;

namespace Realm.Resources.Nametags;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] nametags { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.Nametags.Lua.nametags.lua", Assembly);
}