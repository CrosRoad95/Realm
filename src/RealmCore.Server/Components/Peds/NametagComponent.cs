namespace RealmCore.Server.Components.Peds;

public sealed class NametagComponent : Component
{
    private readonly object _lock = new();

    public event Action<NametagComponent, string>? TextChanged;

    private string _text;
    public string Text
    {
        get
        {
            ThrowIfDisposed();
            return _text;
        }
        set
        {
            ThrowIfDisposed();
            lock (_lock)
            {
                _text = value;
                TextChanged?.Invoke(this, _text);
            }
        }
    }

    public NametagComponent(string text)
    {
        _text = text;
    }
}
