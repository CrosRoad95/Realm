﻿namespace RealmCore.Server.Components.Common;

[ComponentUsage(true)]
public class AttachedEntityComponent : Component
{
    private readonly BoneId _boneId;
    private readonly Vector3? _positionOffset;
    private readonly Vector3? _rotationOffset;

    [Inject]
    private BoneAttachService BoneAttachService { get; set; } = default!;

    private Entity? _attachedEntity;
    public Entity? AttachedEntity
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

    protected override void Load()
    {
        AttachedEntity.Disposed += HandleAttachedEntityDestroyed;
        var element = AttachedEntity.GetRequiredComponent<ElementComponent>().Element;
        var ped = Entity.GetRequiredComponent<PedElementComponent>().Ped;
        BoneAttachService.Attach(element, ped, _boneId, _positionOffset, _rotationOffset);
        AttachedEntity.GetRequiredComponent<ElementComponent>().AreCollisionsEnabled = false;
    }

    public override void Dispose()
    {
        var element = AttachedEntity.GetRequiredComponent<ElementComponent>().Element;
        AttachedEntity.Disposed -= HandleAttachedEntityDestroyed;
        AttachedEntity = null;
        if (BoneAttachService.IsAttached(element))
            BoneAttachService.Detach(element);
        base.Dispose();
    }
}