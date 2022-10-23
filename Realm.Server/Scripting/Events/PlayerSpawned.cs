namespace Realm.Server.Scripting.Events;

public class PlayerSpawned
{
    public RPGPlayer Player { get; set; }
    public Spawn Spawn { get; set; }
}
