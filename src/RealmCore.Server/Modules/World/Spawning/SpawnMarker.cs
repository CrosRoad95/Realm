namespace RealmCore.Server.Modules.World.Spawning;

public abstract class SpawnMarker
{
    protected SpawnMarker(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
