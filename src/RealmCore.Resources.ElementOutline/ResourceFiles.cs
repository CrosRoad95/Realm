using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.ElementOutline;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Outline { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.ElementOutline.Lua.outline.lua", Assembly);
}
