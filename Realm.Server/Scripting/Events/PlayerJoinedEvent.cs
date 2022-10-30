namespace Realm.Server.Scripting.Events;

public class PlayerJoinedEvent : INamedLuaEvent
{
    public static string EventName => "onPlayerJoin";

    public RPGPlayer Player { get; set; }
}
