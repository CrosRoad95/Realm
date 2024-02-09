namespace RealmCore.Server.Modules.Players.Gui.Dx;

public interface IGuiHandlers
{
    Task HandleForm(IFormContext formContext);
    Task HandleAction(IActionContext actionContext);
}

public abstract class DxGui : IPlayerGui
{
    public RealmPlayer Player { get; }
    protected readonly string _name;
    protected readonly bool _cursorLess;
    public string Name => _name;
    public bool CursorLess => _cursorLess;

    protected DxGui(RealmPlayer player, string name, bool cursorLess)
    {
        Player = player;
        _name = name;
        _cursorLess = cursorLess;
    }

    public virtual void Dispose() { }
}
