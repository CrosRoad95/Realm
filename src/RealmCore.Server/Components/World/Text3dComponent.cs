namespace RealmCore.Server.Components.World;

public sealed class Text3dComponent : Component
{
    [Inject]
    private Text3dService Text3dService { get; set; } = default!;

    private readonly string _text;
    private readonly Vector3? _offset;
    private int? _text3dId = null;

    public Text3dComponent(string text, Vector3? offset = null)
    {
        _text = text;
        _offset = offset;
    }

    protected override void Load()
    {
        _text3dId = Text3dService.CreateText3d(Entity.Transform.Position + (_offset ?? Vector3.Zero), _text);
    }

    public override void Dispose()
    {
        base.Dispose();
        if (_text3dId != null)
            Text3dService.RemoveText3d(_text3dId.Value);
    }
}
