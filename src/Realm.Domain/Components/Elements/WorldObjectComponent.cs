namespace Realm.Domain.Components.Elements;

public class WorldObjectComponent : ElementComponent
{
    protected readonly WorldObject _worldObject;
    internal override Element Element => _worldObject;

    internal WorldObjectComponent(WorldObject worldObject)
    {
        _worldObject = worldObject;
    }
}
