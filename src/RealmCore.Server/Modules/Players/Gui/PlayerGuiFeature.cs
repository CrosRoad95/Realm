namespace RealmCore.Server.Modules.Players.Gui;

public interface IPlayerGui : IDisposable
{
    RealmPlayer Player { get; }
}

public interface IPlayerGuiFeature : IPlayerFeature
{
    IPlayerGui? Current { get; set; }

    event Action<IPlayerGuiFeature, RealmPlayer, IPlayerGui?, IPlayerGui?>? Changed;

    bool TryClose<TGui>() where TGui : IPlayerGui;
    bool TryClose();
    TGui SetCurrentWithDI<TGui>(params object[] parameters) where TGui : IPlayerGui;
}

internal sealed class PlayerGuiFeature : IPlayerGuiFeature, IDisposable
{
    private readonly object _lock = new();
    private readonly IPlayerUserFeature _userFeature;
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
    public PlayerGuiFeature(PlayerContext playerContext, IPlayerUserFeature userFeature)
    {
        _userFeature = userFeature;
        Player = playerContext.Player;
        userFeature.LoggedOut += HandleSignedOut;
    }

    private void HandleSignedOut(IPlayerUserFeature userFeature, RealmPlayer player)
    {
        TryClose();
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

    public bool TryClose<TGui>() where TGui : IPlayerGui
    {
        lock (_lock)
        {
            if (Current is TGui)
            {
                Current = null;
                return true;
            }
        }
        return false;
    }

    public bool TryClose()
    {
        lock (_lock)
        {
            if(Current != null)
            {
                Current = null;
                return true;
            }
            return false;
        }
    }

    public void Dispose()
    {
        _userFeature.LoggedOut -= HandleSignedOut;
    }
}
