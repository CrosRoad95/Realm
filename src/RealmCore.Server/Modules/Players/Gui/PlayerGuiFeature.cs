namespace RealmCore.Server.Modules.Players.Gui;

public interface IPlayerGui : IDisposable
{
    RealmPlayer Player { get; }
}

public sealed class PlayerGuiFeature : IPlayerFeature, IDisposable
{
    private readonly object _lock = new();
    private readonly PlayerUserFeature _userFeature;
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

    public event Action<PlayerGuiFeature, RealmPlayer, IPlayerGui?, IPlayerGui?>? Changed;
    public PlayerGuiFeature(PlayerContext playerContext, PlayerUserFeature userFeature)
    {
        _userFeature = userFeature;
        Player = playerContext.Player;
        userFeature.LoggedOut += HandleLoggedOut;
    }

    private Task HandleLoggedOut(object? sender, PlayerLoggedOutEventArgs args)
    {
        TryClose();

        return Task.CompletedTask;
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
        _userFeature.LoggedOut -= HandleLoggedOut;
    }
}
