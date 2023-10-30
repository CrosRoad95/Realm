namespace RealmCore.Server.Components.World;

public sealed class Text3dComponent : Component
{
    private readonly string _text;

    internal int? Text3dId { get; set; }

    public string Text => _text;

    public Vector3 Position { get; }

    public Text3dComponent(string text, Vector3 position)
    {
        _text = text;
        Position = position;
    }
}
