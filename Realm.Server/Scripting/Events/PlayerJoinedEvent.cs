namespace Realm.Server.Scripting.Events;

public class PlayerJoinedEvent : INamedLuaEvent
{
    public static string Name => "onPlayerJoin";

    public RPGPlayer Player { get; set; }
}
