namespace RealmCore.Server.Components.Peds;

public sealed class NametagComponent : Component
{
    [Inject]
    private INametagsService NametagsService { get; set; } = default!;

    private readonly object _lock = new();
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
                NametagsService.SetNametag((Ped)Entity.Element, _text);
            }
        }
    }

    public NametagComponent(string text)
    {
        _text = text;
    }

    protected override void Load()
    {
        NametagsService.SetNametag((Ped)Entity.Element, _text);
    }

    public override void Dispose()
    {
        base.Dispose();
        NametagsService.RemoveNametag((Ped)Entity.Element);
    }
}
