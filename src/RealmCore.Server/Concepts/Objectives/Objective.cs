namespace RealmCore.Server.Concepts.Objectives;

public abstract class Objective : IDisposable
{
    private bool _isFulfilled = false;
    private RealmPlayer? _player;
    private IElementFactory? _elementFactory;

    public bool IsFulfilled => _isFulfilled;

    public event Action<Objective, object?>? Completed;
    public event Action<Objective>? InCompleted;
    public event Action<Objective>? Disposed;

    public RealmPlayer Player { get => _player ?? throw new InvalidOperationException(); internal set => _player = value; }

    private Blip? _blip;
    public abstract Vector3 Position { get; }
    protected ILogger Logger { get; set; } = default!;
    protected abstract void Load(RealmPlayer player);
    public virtual void Update() { }

    internal void LoadInternal(RealmPlayer player)
    {
        Logger = player.ServiceProvider.GetRequiredService<ILogger>();
        _elementFactory = player.ServiceProvider.GetRequiredService<IElementFactory>();
        Load(player);
    }

    protected void Complete(Objective objective, object? data = null)
    {
        if (Player == null)
            throw new ArgumentNullException(nameof(Player), "Element cannot be null.");

        if (_isFulfilled)
            throw new ObjectiveAlreadyFulfilledException();

        RemoveBlip();

        Completed?.Invoke(objective, data);
        _isFulfilled = true;
    }

    public void Incomplete(Objective objective)
    {
        if (Player == null)
            throw new ArgumentNullException(nameof(Player), "Element cannot be null.");

        if (_isFulfilled)
            throw new ObjectiveAlreadyFulfilledException();

        RemoveBlip();

        InCompleted?.Invoke(objective);
        _isFulfilled = true;
    }

    public void AddBlip(BlipIcon blipIcon)
    {
        if (_blip != null)
            throw new InvalidOperationException();

        // TODO:
    }

    public void RemoveBlip()
    {
        if (_blip != null && _blip.Destroy())
            _blip = null;
    }

    public virtual void Dispose()
    {
        RemoveBlip();

        Disposed?.Invoke(this);
    }
}
