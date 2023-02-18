namespace Realm.Domain.Components.Elements;

public class WorldObjectComponent : ElementComponent
{
    protected readonly WorldObject _worldObject;
    internal override Element Element => _worldObject;

    public ushort Model { get => _worldObject.Model; set => _worldObject.Model = value; }
    internal WorldObjectComponent(WorldObject worldObject)
    {
        _worldObject = worldObject;
    }
}
