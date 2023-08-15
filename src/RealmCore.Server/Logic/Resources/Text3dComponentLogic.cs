namespace RealmCore.Server.Logic.Resources;

internal class Text3dComponentLogic : ComponentLogic<Text3dComponent>
{
    private readonly Text3dService _text3DService;

    public Text3dComponentLogic(Text3dService text3DService, IECS ecs) : base(ecs)
    {
        _text3DService = text3DService;
    }

    protected override void ComponentAdded(Text3dComponent text3DComponent)
    {
        text3DComponent.Text3dId = _text3DService.CreateText3d(text3DComponent.Entity.Transform.Position + text3DComponent.Offset, text3DComponent.Text);
    }

    protected override void ComponentRemoved(Text3dComponent text3DComponent)
    {
        if(text3DComponent.Text3dId != null)
        {
            _text3DService.RemoveText3d(text3DComponent.Text3dId.Value);
            text3DComponent.Text3dId = null;
        }
    }
}
