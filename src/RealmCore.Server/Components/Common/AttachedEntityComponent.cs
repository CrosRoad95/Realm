namespace RealmCore.Server.Components.Common;

[ComponentUsage(true)]
public class AttachedEntityComponent : Component
{
    private readonly BoneId _boneId;
    private readonly Vector3? _positionOffset;
    private readonly Vector3? _rotationOffset;
    private readonly object _lock = new();

    [Inject]
    private BoneAttachService BoneAttachService { get; set; } = default!;

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
        var element = _attachedEntity.GetRequiredComponent<ElementComponent>().Element;
        var ped = Entity.GetRequiredComponent<PedElementComponent>().Ped;
        BoneAttachService.Attach(element, ped, _boneId, _positionOffset, _rotationOffset);
        _attachedEntity.GetRequiredComponent<ElementComponent>().AreCollisionsEnabled = false;
    }

    public override void Dispose()
    {
        var element = _attachedEntity.GetRequiredComponent<ElementComponent>().Element;
        _attachedEntity.Disposed -= HandleAttachedEntityDestroyed;
        if (BoneAttachService.IsAttached(element))
            BoneAttachService.Detach(element);
        base.Dispose();
    }
}
