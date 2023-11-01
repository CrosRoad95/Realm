using RealmCore.Resources.Base.Interfaces;

namespace RealmCore.Server.Extensions;

public static class LuaEventHubExtensions
{
    public static void Invoke<THub>(this ILuaEventHub<THub> luaEventHub, RealmPlayer player, Expression<Action<THub>> expression)
    {
        luaEventHub.Invoke(player, expression);
    }
}
