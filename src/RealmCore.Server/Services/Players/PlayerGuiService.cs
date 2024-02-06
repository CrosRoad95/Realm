namespace RealmCore.Server.Services.Players;

public interface IPlayerGui : IDisposable
{
    RealmPlayer Player { get; }
}

public interface IPlayerGuiService : IPlayerService
{
    IPlayerGui? Current { get; set; }

    event Action<IPlayerGuiService, RealmPlayer, IPlayerGui?, IPlayerGui?>? Changed;

    void Close<TGui>() where TGui : IPlayerGui;
    TGui SetCurrentWithDI<TGui>(params object[] parameters) where TGui : IPlayerGui;
}

internal sealed class PlayerGuiService : IPlayerGuiService
{
    private IPlayerGui? _current;

    public IPlayerGui? Current
    {
        get => _current; set
        {
            if (_current == value)
                return;

            _current?.Dispose();

            if(value != null && value?.Player == null)
                throw new NullReferenceException(nameof(value.Player));

            var _old = _current;
            _current = value;
            Changed?.Invoke(this, Player, _old, value);
        }
    }

    public RealmPlayer Player { get; private set; }

    public event Action<IPlayerGuiService, RealmPlayer, IPlayerGui?, IPlayerGui?>? Changed;
    public PlayerGuiService(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public TGui SetCurrentWithDI<TGui>(params object[] parameters) where TGui: IPlayerGui
    {
        var gui = ActivatorUtilities.CreateInstance<TGui>(Player.ServiceProvider, parameters);
        Current = gui;
        return gui;
    }

    public void Close<TGui>() where TGui : IPlayerGui
    {
        if (Player.Gui.Current is TGui)
            Player.Gui.Current = null;
    }
}
