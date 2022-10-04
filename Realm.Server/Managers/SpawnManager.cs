namespace Realm.Server.Managers;

internal class SpawnManager : ISpawnManager
{
    private struct Spawn
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Guid Id { get; set; }
    }

    private readonly List<Spawn> _spawns = new();
    public SpawnManager() { }

    public Guid CreateSpawn(string name, Vector3 position, Vector3 rotation)
    {
        var spawn = new Spawn
        {
            Name = name,
            Position = position,
            Rotation = rotation,
            Id = Guid.NewGuid(),
        };
        _spawns.Add(spawn);
        return spawn.Id;
    }
}
