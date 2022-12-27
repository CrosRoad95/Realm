using SlipeServer.Resources.Text3d;

namespace Realm.Domain.Components.World;

public sealed class Text3dComponent : Component
{
    private readonly string _text;
    private readonly Vector3? _offset;
    private int? _text3dId = null;

    public Text3dComponent(string text, Vector3? offset = null)
    {
        _text = text;
        _offset = offset;
    }

    public override Task Load()
    {
        var text3DService = Entity.GetRequiredService<Text3dService>();
        _text3dId = text3DService.CreateText3d(Entity.Transform.Position + _offset ?? Vector3.Zero, _text);
        return Task.CompletedTask;
    }

    public override void Destroy()
    {
        if (_text3dId == null)
            throw new Exception("Bug?");
        var text3DService = Entity.GetRequiredService<Text3dService>();
        text3DService.RemoveText3d(_text3dId.Value);
    }
}
