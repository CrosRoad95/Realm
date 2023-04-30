namespace RealmCore.Server.Concepts.Spawning;

public sealed class DirectionalSpawnMarker : SpawnMarker
{
    public Vector3 Position { get; }
    public float Direction { get; }

    public DirectionalSpawnMarker(string name, Vector3 position, float direction) : base(name)
    {
        Position = position;
        Direction = direction;
    }
}
