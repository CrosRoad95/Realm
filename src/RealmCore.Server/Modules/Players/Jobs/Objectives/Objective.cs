namespace RealmCore.Server.Modules.Players.Jobs.Objectives;

public abstract class Objective : IDisposable
{
    private bool _isFulfilled = false;
    private RealmPlayer? _player;
    private IScopedElementFactory? _elementFactory;
    protected IScopedElementFactory ElementFactory => _elementFactory ?? throw new InvalidOperationException();

    public bool IsFulfilled => _isFulfilled;

    public event Action<Objective, object?>? Completed;
    public event Action<Objective>? InCompleted;
    public event Action<Objective>? Disposed;

    public RealmPlayer Player { get => _player ?? throw new InvalidOperationException(); internal set => _player = value; }

    private Blip? _blip;
    public abstract Location Location { get; }
    protected abstract void Load();
    public virtual void Update() { }

    internal void LoadInternal(RealmPlayer player)
    {
        Player = player;
        _elementFactory = player.ElementFactory.CreateScope();
        Load();
    }

    protected void Complete(Objective objective, object? data = null)
    {
        if (Player == null)
            throw new ArgumentNullException(nameof(Player), "Element cannot be null.");

        if (_isFulfilled)
            throw new ObjectiveAlreadyFulfilledException();

        Completed?.Invoke(objective, data);
        _isFulfilled = true;
    }

    public void Incomplete(Objective objective)
    {
        if (Player == null)
            throw new ArgumentNullException(nameof(Player), "Element cannot be null.");

        if (_isFulfilled)
            throw new ObjectiveAlreadyFulfilledException();

        InCompleted?.Invoke(objective);
        _isFulfilled = true;
    }

    public void AddBlip(BlipIcon blipIcon)
    {
        if (_blip != null)
            throw new InvalidOperationException();

        _blip = ElementFactory.CreateBlip(Location, blipIcon);
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
