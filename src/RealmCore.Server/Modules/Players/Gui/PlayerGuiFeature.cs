namespace RealmCore.Server.Modules.Players.Gui;

public interface IPlayerGui : IDisposable
{
    RealmPlayer Player { get; }
}

public interface IPlayerGuiFeature : IPlayerFeature
{
    IPlayerGui? Current { get; set; }

    event Action<IPlayerGuiFeature, RealmPlayer, IPlayerGui?, IPlayerGui?>? Changed;

    void Close<TGui>() where TGui : IPlayerGui;
    TGui SetCurrentWithDI<TGui>(params object[] parameters) where TGui : IPlayerGui;
}

internal sealed class PlayerGuiFeature : IPlayerGuiFeature
{
    private readonly object _lock = new();
    private IPlayerGui? _current;

    public IPlayerGui? Current
    {
        get => _current; set
        {
            lock (_lock)
            {
                if (_current == value)
                    return;

                _current?.Dispose();

                if (value != null && value?.Player == null)
                    throw new NullReferenceException(nameof(value.Player));

                var _old = _current;
                _current = value;
                Changed?.Invoke(this, Player, _old, value);
            }
        }
    }

    public RealmPlayer Player { get; init; }

    public event Action<IPlayerGuiFeature, RealmPlayer, IPlayerGui?, IPlayerGui?>? Changed;
    public PlayerGuiFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public TGui SetCurrentWithDI<TGui>(params object[] parameters) where TGui : IPlayerGui
    {
        lock (_lock)
        {
            var gui = ActivatorUtilities.CreateInstance<TGui>(Player.ServiceProvider, parameters);
            Current = gui;
            return gui;
        }
    }

    public void Close<TGui>() where TGui : IPlayerGui
    {
        lock (_lock)
        {
            if (Player.Gui.Current is TGui)
                Player.Gui.Current = null;
        }
    }
}
