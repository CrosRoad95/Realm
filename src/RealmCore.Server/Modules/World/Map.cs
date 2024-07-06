namespace RealmCore.Server.Modules.World;

public readonly record struct RemoveWorldModel(ushort model, Vector3 position, float radius, byte interior = 0);

public abstract class MapBase : IDisposable
{
    protected readonly IElementIdGenerator _elementIdGenerator;
    private bool _disposed = false;

    public event Action<MapBase>? Disposed;

    public BoundingBox BoundingBox { get; protected set; }
    public MapBase(IElementIdGenerator elementIdGenerator)
    {
        _elementIdGenerator = elementIdGenerator;
    }

    public virtual bool LoadFor(RealmPlayer player) { return false; }

    public virtual bool UnloadFor(RealmPlayer player) { return false; }

    public virtual bool Load() { return false; }

    public virtual bool Unload() { return false; }

    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose()
    {
        ThrowIfDisposed();
        Disposed?.Invoke(this);

        _disposed = true;
    }
}

public sealed class Map : MapBase
{
    private readonly MtaServer _mtaServer;
    private readonly GameWorld _gameWorld;
    private readonly RemoveWorldModel[] _removeWorldModels;
    private readonly List<WorldObject> _worldObjects = [];
    public string? Name { get; set; }

    public Map(IElementIdGenerator elementIdGenerator, MtaServer mtaServer, GameWorld gameWorld, IEnumerable<WorldObject> worldObjects, IEnumerable<RemoveWorldModel> removeWorldModels) : base(elementIdGenerator)
    {
        Vector3 min = Vector3.Zero;
        Vector3 max = Vector3.Zero;
        foreach (var worldObject in worldObjects)
        {
            worldObject.Id = (ElementId)elementIdGenerator.GetId();
            var pos = worldObject.Position;
            if (pos.X < min.X) min.X = pos.X;
            if (pos.X > max.X) max.X = pos.X;
            if (pos.Y < min.Y) min.Y = pos.Y;
            if (pos.Y > max.Y) max.Y = pos.Y;
            if (pos.Z < min.Z) min.Z = pos.Z;
            if (pos.X > max.Z) max.Z = pos.Z;
            _worldObjects.Add(worldObject);
        }

        BoundingBox = new BoundingBox((min + max) * 0.5f, max - min);
        _mtaServer = mtaServer;
        _gameWorld = gameWorld;
        _removeWorldModels = removeWorldModels.ToArray();
    }

    // TODO: Add check, load for specific player or for all
    public override bool LoadFor(RealmPlayer player)
    {
        ThrowIfDisposed();
        foreach (var worldObject in _worldObjects)
            worldObject.CreateFor(player);
        return true;
    }

    public override bool UnloadFor(RealmPlayer player)
    {
        ThrowIfDisposed();
        foreach (var worldObject in _worldObjects)
            worldObject.DestroyFor(player);

        return true;
    }

    public override bool Load()
    {
        ThrowIfDisposed();
        foreach (var worldObject in _worldObjects)
            worldObject.AssociateWith(_mtaServer);
        foreach (var removeWorldModel in _removeWorldModels)
            _gameWorld.RemoveWorldModel(removeWorldModel.model, removeWorldModel.position, removeWorldModel.radius, removeWorldModel.interior);

        return true;
    }

    public override bool Unload()
    {
        ThrowIfDisposed();
        foreach (var worldObject in _worldObjects)
            worldObject.Destroy();
        foreach (var removeWorldModel in _removeWorldModels)
            _gameWorld.RestoreWorldModel(removeWorldModel.model, removeWorldModel.position, removeWorldModel.radius, removeWorldModel.interior);

        return true;
    }
}
