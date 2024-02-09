namespace RealmCore.Server.Modules.World.Spawning;

public sealed class PointSpawnMarker : SpawnMarker
{
    public Vector3 Position { get; }

    public PointSpawnMarker(string name, Vector3 position) : base(name)
    {
        Position = position;
    }
}
