namespace Realm.Domain.Components.Common;

public class AttachedEntityComponent : Component
{
    public Entity AttachedEntity { get; }
    public Vector3 Offset { get; }

    public AttachedEntityComponent(Entity entity, Vector3 offset)
    {
        AttachedEntity = entity;
        Offset = offset;
    }

    protected override void Load()
    {
        AttachedEntity.GetRequiredComponent<ElementComponent>().AreCollisionsEnabled = false;
        Entity.Transform.PositionChanged += Transform_PositionChanged;
        Update();
    }

    private void Transform_PositionChanged(Transform transform)
    {
        Update();
    }

    private void Update()
    {
        AttachedEntity.Transform.Position = Entity.Transform.Position + Entity.Transform.Forward * Offset;
    }

    public override void Dispose()
    {
        Entity.Transform.PositionChanged -= Transform_PositionChanged;
        AttachedEntity.GetRequiredComponent<ElementComponent>().AreCollisionsEnabled = true;
    }
}
