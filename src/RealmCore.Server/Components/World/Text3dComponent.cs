namespace RealmCore.Server.Components.World;

public sealed class Text3dComponent : Component
{
    private readonly string _text;
    private readonly Vector3 _offset;
    internal int? Text3dId { get; set; }

    public string Text => _text;
    public Vector3 Offset => _offset;

    public Text3dComponent(string text, Vector3? offset = null)
    {
        _text = text;
        _offset = offset ?? Vector3.Zero;
    }
}
