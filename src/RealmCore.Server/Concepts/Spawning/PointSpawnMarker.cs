namespace RealmCore.Server.Concepts.Spawning;

public sealed class PointSpawnMarker : SpawnMarker
{
    public Vector3 Position { get; }

    public PointSpawnMarker(Vector3 position)
    {
        Position = position;
    }
}
