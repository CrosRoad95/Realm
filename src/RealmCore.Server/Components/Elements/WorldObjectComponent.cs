namespace RealmCore.Server.Components.Elements;

public class WorldObjectComponent : ElementComponent
{
    protected readonly WorldObject _worldObject;
    internal override Element Element => _worldObject;

    public ushort Model
    {
        get
        {
            ThrowIfDisposed();
            return _worldObject.Model;
        }
        set
        {
            ThrowIfDisposed();
            _worldObject.Model = value;
        }
    }

    internal WorldObjectComponent(WorldObject worldObject)
    {
        _worldObject = worldObject;
    }
}
