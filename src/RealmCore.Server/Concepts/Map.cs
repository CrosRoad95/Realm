namespace RealmCore.Server.Concepts;

internal sealed class Map : IMap
{
    private readonly List<WorldObject> _worldObjects;
    private readonly BoundingBox _boundingBox;
    public List<WorldObject> WorldObjects => _worldObjects;

    public Map(MapIdGenerator mapIdGenerator, IEnumerable<WorldObject> worldObjects)
    {
        if (worldObjects.TryGetNonEnumeratedCount(out int count))
        {
            _worldObjects = new(count);
        }
        else
        {
            _worldObjects = new(worldObjects.Count());
        }

        Vector3 min = Vector3.Zero;
        Vector3 max = Vector3.Zero;
        foreach (var worldObject in worldObjects)
        {
            worldObject.Id = (ElementId)mapIdGenerator.GetId();
            var pos = worldObject.Position;
            if (pos.X < min.X) min.X = pos.X;
            if (pos.X > max.X) max.X = pos.X;
            if (pos.Y < min.Y) min.Y = pos.X;
            if (pos.Y > max.Y) max.Y = pos.X;
            if (pos.Z < min.Z) min.Z = pos.X;
            if (pos.X > max.Z) max.Z = pos.X;
            _worldObjects.Add(worldObject);
        }

        _boundingBox = new BoundingBox((min + max) * 0.5f, max - min);
    }

    public void LoadForPlayer(Player player)
    {
        foreach (var worldObject in _worldObjects)
            worldObject.CreateFor(player);
    }
}
