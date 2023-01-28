using System.Security.Cryptography.Xml;

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

    public override void Load()
    {
        Entity.GetRequiredComponent<ElementComponent>().AreCollisionsEnabled = false;
        Entity.Transform.PositionChanged += Transform_PositionChanged;
        Update();
    }

    private void Transform_PositionChanged(Transform transform)
    {
        AttachedEntity.Transform.Position = transform.Position + Entity.Transform.Forward * Offset;
    }

    private void Update()
    {
        AttachedEntity.Transform.Position = Entity.Transform.Position + Offset;
    }

    public override void Dispose()
    {
        Entity.Transform.PositionChanged -= Transform_PositionChanged;
        Entity.GetRequiredComponent<ElementComponent>().AreCollisionsEnabled = true;
    }
}
