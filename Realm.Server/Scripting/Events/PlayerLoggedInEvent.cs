namespace Realm.Server.Scripting.Events;

public class PlayerLoggedInEvent
{
    public RPGPlayer Player { get; init; }
    public User Account { get; init; }
}
