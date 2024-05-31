namespace RealmCore.Server.Modules.Players.Gui.Browser;

public abstract class BrowserGui : IPlayerGui
{
    public RealmPlayer Player { get; }
    public string Path { get; }
    public IReadOnlyDictionary<string, string?>? QueryParameters { get; }

    public event Action<BrowserGui>? Changed;

    public BrowserGui(RealmPlayer player, string path, Dictionary<string, string?>? queryParameters = null)
    {
        Player = player;
        Path = path;
        QueryParameters = queryParameters;
    }

    protected void StateHasChanged()
    {
        Changed?.Invoke(this);
    }

    public virtual void Dispose() { }
}