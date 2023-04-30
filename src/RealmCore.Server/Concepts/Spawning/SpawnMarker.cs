namespace RealmCore.Server.Concepts.Spawning;

public abstract class SpawnMarker
{
    protected SpawnMarker(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
