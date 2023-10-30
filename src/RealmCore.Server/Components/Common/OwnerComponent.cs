namespace RealmCore.Server.Components.Common;

public class OwnerComponent : Component
{
    public Entity OwningEntity { get; }

    public OwnerComponent(Entity owningEntity)
    {
        OwningEntity = owningEntity;
    }
}
