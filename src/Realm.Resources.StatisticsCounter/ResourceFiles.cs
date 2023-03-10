using SlipeServer.Resources.Base;
using System.Reflection;

namespace Realm.Resources.StatisticsCounter;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Counter { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.StatisticsCounter.Lua.counter.lua", Assembly);
    public static byte[] FpsCounter { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.StatisticsCounter.Lua.fpsCounter.lua", Assembly);
}
