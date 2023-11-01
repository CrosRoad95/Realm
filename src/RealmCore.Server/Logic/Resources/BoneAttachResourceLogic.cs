namespace RealmCore.Server.Logic.Resources;

internal sealed class BoneAttachResourceLogic : ComponentLogic<AttachedElementComponent>
{
    private readonly BoneAttachService _boneAttachService;

    public BoneAttachResourceLogic(IElementFactory elementFactory, BoneAttachService boneAttachService) : base(elementFactory)
    {
        _boneAttachService = boneAttachService;
    }

    protected override void ComponentAdded(AttachedElementComponent attachedEntityComponent)
    {
        var element = attachedEntityComponent.AttachedElement;
        var ped = (Ped)element;
        _boneAttachService.Attach(element, ped, attachedEntityComponent.BoneId, attachedEntityComponent.PositionOffset, attachedEntityComponent.RotationOffset);
        element.AreCollisionsEnabled = false;
        attachedEntityComponent.Detached += HandleDetachedFromEntity;
    }

    private void HandleDetachedFromEntity(IComponentLifecycle component)
    {
        component.Detached -= HandleDetachedFromEntity;
        if(component is AttachedElementComponent attachedEntityComponent)
        {
            if (_boneAttachService.IsAttached(attachedEntityComponent.Element))
                _boneAttachService.Detach(attachedEntityComponent.Element);
        }
        // bug?
    }
}
