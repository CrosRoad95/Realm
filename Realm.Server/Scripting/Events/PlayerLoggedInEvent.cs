namespace Realm.Server.Scripting.Events;

public class PlayerLoggedInEvent : INamedLuaEvent
{
    public static string Name => "onPlayerLogin";

    public RPGPlayer Player { get; init; }
    public PlayerAccount Account { get; init; }
}
