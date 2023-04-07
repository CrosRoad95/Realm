namespace RealmCore.Server.Extensions;

public static class LuaEventHubExtensions
{
    public static void Invoke<THub>(this ILuaEventHub<THub> luaEventHub, Entity entity, Expression<Action<THub>> expression)
    {
        luaEventHub.Invoke(entity.Player, expression);
    }
}
