namespace RealmCore.Server.Logic.Components;

internal sealed class Text3dComponentLogic : ComponentLogic<Text3dComponent>
{
    private readonly Text3dService _text3DService;

    public Text3dComponentLogic(Text3dService text3DService, IEntityEngine entityEngine) : base(entityEngine)
    {
        _text3DService = text3DService;
    }

    protected override void ComponentAdded(Text3dComponent text3DComponent)
    {
        text3DComponent.Text3dId = _text3DService.CreateText3d(text3DComponent.Position, text3DComponent.Text);
    }

    protected override void ComponentDetached(Text3dComponent text3DComponent)
    {
        if (text3DComponent.Text3dId != null)
        {
            _text3DService.RemoveText3d(text3DComponent.Text3dId.Value);
            text3DComponent.Text3dId = null;
        }
    }
}
