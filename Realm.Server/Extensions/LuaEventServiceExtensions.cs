using Realm.Domain.Elements;

namespace Realm.Server.Extensions;

public static class LuaEventExtensions
{
    public static void Response(this LuaEvent luaEvent, params object[] parameters)
    {
        ((RPGPlayer)luaEvent.Player).TriggerClientEvent($"{luaEvent.Name}Response", parameters);
    }
}
