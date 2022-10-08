namespace Realm.Scripting.Classes.Events;

public class PlayerJoinedEvent
{
    public IRPGPlayer Player { get; set; }

    public override string ToString() => "PlayerJoinedEvent";
}
