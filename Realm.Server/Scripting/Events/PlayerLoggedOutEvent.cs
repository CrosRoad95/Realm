namespace Realm.Server.Scripting.Events;

public class PlayerLoggedOutEvent : INamedLuaEvent
{
    public static string EventName => "onPlayerLogout";

    public RPGPlayer Player { get; init; }
}
