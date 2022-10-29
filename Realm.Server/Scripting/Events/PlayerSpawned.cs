namespace Realm.Server.Scripting.Events;

public class PlayerSpawned : INamedLuaEvent
{
    public static string Name => "onPlayerSpawn";

    public RPGPlayer Player { get; set; }
    public Spawn Spawn { get; set; }
}
