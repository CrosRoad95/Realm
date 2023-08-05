namespace RealmCore.Server.Components.Common;

[ComponentUsage(true)]
public class AttachedEntityComponent : Component
{
    private readonly BoneId _boneId;
    private readonly Vector3? _positionOffset;
    private readonly Vector3? _rotationOffset;
    private readonly object _lock = new();

    private Entity _attachedEntity;
    public Entity AttachedEntity
    {
        get
        {
            ThrowIfDisposed();
            return _attachedEntity;
        }
        private set
        {
            ThrowIfDisposed();
            _attachedEntity = value;
        }
    }

    public BoneId BoneId => _boneId;

    public Vector3? PositionOffset => _positionOffset;

    public Vector3? RotationOffset => _rotationOffset;

    public AttachedEntityComponent(Entity entity, BoneId boneId, Vector3? positionOffset = null, Vector3? rotationOffset = null)
    {
        _attachedEntity = entity;
        _boneId = boneId;
        _positionOffset = positionOffset;
        _rotationOffset = rotationOffset;
    }

    private void HandleAttachedEntityDestroyed(Entity entity)
    {
        Entity.TryDestroyComponent(this);
    }

    public bool TryDetach(out Entity entity)
    {
        lock (_lock)
        {
            if(_disposed)
            {
                entity = null;
                return false;
            }
            entity = _attachedEntity;
            return Entity.TryDestroyComponent(this);
        }
    }

    protected override void Load()
    {
        _attachedEntity.Disposed += HandleAttachedEntityDestroyed;
    }

    public override void Dispose()
    {
        _attachedEntity.Disposed -= HandleAttachedEntityDestroyed;
    }
}
