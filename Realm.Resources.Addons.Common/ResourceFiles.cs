using SlipeServer.Resources.Base;
using System.Reflection;

namespace Realm.Resources.Addons.Common;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Utilities { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.Addons.Common.Lua.utilities.lua", Assembly);
}
