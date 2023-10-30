namespace RealmCore.Server.Components.Common;

[ComponentUsage(true)]
public class AttachedEntityComponent : ComponentLifecycle
{
    private readonly BoneId _boneId;
    private readonly Vector3? _positionOffset;
    private readonly Vector3? _rotationOffset;
    private readonly object _lock = new();
    public Entity? AttachedEntity { get; set; }

    public BoneId BoneId => _boneId;

    public Vector3? PositionOffset => _positionOffset;

    public Vector3? RotationOffset => _rotationOffset;

    public AttachedEntityComponent(Entity entity, BoneId boneId, Vector3? positionOffset = null, Vector3? rotationOffset = null)
    {
        AttachedEntity = entity;
        _boneId = boneId;
        _positionOffset = positionOffset;
        _rotationOffset = rotationOffset;
    }

    public bool TryDetach(out Entity? entity)
    {
        lock (_lock)
        {
            if(AttachedEntity != null)
            {
                entity = AttachedEntity;
                AttachedEntity = null;
                return true;
            }
            Entity.TryDestroyComponent(this);
            entity = null;
            return false;
        }
    }
}
