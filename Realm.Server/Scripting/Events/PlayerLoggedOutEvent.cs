namespace Realm.Server.Scripting.Events;

public class PlayerLoggedOutEvent : INamedLuaEvent
{
    public static string Name => "onPlayerLogout";

    public RPGPlayer Player { get; init; }
}
