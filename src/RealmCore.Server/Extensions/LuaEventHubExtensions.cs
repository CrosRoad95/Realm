using RealmCore.ECS;
using RealmCore.Resources.Base.Interfaces;

namespace RealmCore.Server.Extensions;

public static class LuaEventHubExtensions
{
    public static void Invoke<THub>(this ILuaEventHub<THub> luaEventHub, Entity entity, Expression<Action<THub>> expression)
    {
        luaEventHub.Invoke(entity.GetPlayer(), expression);
    }
}
