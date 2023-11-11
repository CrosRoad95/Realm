namespace RealmCore.Server.Logic.Resources;

internal sealed class BoneAttachResourceLogic : ComponentLogic<AttachedElementComponent>
{
    private readonly BoneAttachService _boneAttachService;

    public BoneAttachResourceLogic(IElementFactory elementFactory, BoneAttachService boneAttachService) : base(elementFactory)
    {
        _boneAttachService = boneAttachService;
    }

    protected override void ComponentAdded(AttachedElementComponent attachedElementComponent)
    {
        var element = attachedElementComponent.AttachedElement;
        _boneAttachService.Attach(attachedElementComponent.AttachedElement, (Ped)attachedElementComponent.Element, attachedElementComponent.BoneId, attachedElementComponent.PositionOffset, attachedElementComponent.RotationOffset);
        element.AreCollisionsEnabled = false;
        attachedElementComponent.Detached += HandleDetachedFromElement;
    }

    private void HandleDetachedFromElement(IComponentLifecycle component)
    {
        component.Detached -= HandleDetachedFromElement;
        if(component is AttachedElementComponent attachedElementComponent)
        {
            if (attachedElementComponent.AttachedElement != null && _boneAttachService.IsAttached(attachedElementComponent.AttachedElement))
                _boneAttachService.Detach(attachedElementComponent.AttachedElement);
        }
        // bug?
    }
}
