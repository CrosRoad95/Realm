using SlipeServer.Resources.Base;
using System.Reflection;

namespace Realm.Resources.Base;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Utilities { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.Base.Lua.utilities.lua", Assembly);
}
