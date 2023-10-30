namespace RealmCore.Server.Logic.Resources;

internal sealed class BoneAttachResourceLogic : ComponentLogic<AttachedEntityComponent>
{
    private readonly BoneAttachService _boneAttachService;

    public BoneAttachResourceLogic(IEntityEngine entityEngine, BoneAttachService boneAttachService) : base(entityEngine)
    {
        _boneAttachService = boneAttachService;
    }

    protected override void ComponentAdded(AttachedEntityComponent attachedEntityComponent)
    {
        if(!attachedEntityComponent.Entity.TryGetComponent(out IElementComponent elementComponent))
        {
            // TODO: not supported
            return;
        }
        var element = (Element)attachedEntityComponent.AttachedEntity.GetRequiredComponent<IElementComponent>();
        var ped = (Ped)elementComponent;
        _boneAttachService.Attach(element, ped, attachedEntityComponent.BoneId, attachedEntityComponent.PositionOffset, attachedEntityComponent.RotationOffset);
        element.AreCollisionsEnabled = false;
        attachedEntityComponent.Detached += HandleDetachedFromEntity;
    }

    private void HandleDetachedFromEntity(IComponentLifecycle component)
    {
        component.Detached -= HandleDetachedFromEntity;
        if(component is AttachedEntityComponent attachedEntityComponent)
        {
            var element = (Element)attachedEntityComponent.Entity.GetRequiredComponent<IElementComponent>();
            if (_boneAttachService.IsAttached(element))
                _boneAttachService.Detach(element);
        }
        // bug?
    }
}
