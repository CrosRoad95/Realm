namespace RealmCore.Server.Logic.Resources;

internal sealed class BoneAttachResourceLogic : ComponentLogic<AttachedEntityComponent>
{
    private readonly BoneAttachService _boneAttachService;

    public BoneAttachResourceLogic(IECS ecs, BoneAttachService boneAttachService) : base(ecs)
    {
        _boneAttachService = boneAttachService;
    }

    protected override void ComponentAdded(AttachedEntityComponent attachedEntityComponent)
    {
        var element = attachedEntityComponent.AttachedEntity.Element;
        if(!attachedEntityComponent.Entity.TryGetComponent(out ElementComponent elementComponent))
        {
            // TODO: not supported
            return;
        }
        var ped = (Ped)elementComponent.Element;
        _boneAttachService.Attach(element, ped, attachedEntityComponent.BoneId, attachedEntityComponent.PositionOffset, attachedEntityComponent.RotationOffset);
        attachedEntityComponent.AttachedEntity.GetRequiredComponent<ElementComponent>().AreCollisionsEnabled = false;
        attachedEntityComponent.DetachedFromEntity += HandleDetachedFromEntity;
    }

    private void HandleDetachedFromEntity(Component component)
    {
        component.DetachedFromEntity -= HandleDetachedFromEntity;
        if(component is AttachedEntityComponent attachedEntityComponent)
        {
            var element = attachedEntityComponent.AttachedEntity.Element;
            if (_boneAttachService.IsAttached(element))
                _boneAttachService.Detach(element);
        }
        // bug?
    }
}
