namespace RealmCore.Server.Components.Players.Abstractions;

public abstract class BrowserGui : IPlayerGui
{
    public RealmPlayer Player { get; }
    public string Path { get; }

    public event Action<BrowserGui>? Changed;
    internal event Action<BrowserGui, BrowserGui>? NavigationRequested;

    public BrowserGui(RealmPlayer player, string path)
    {
        Player = player;
        Path = path;
    }

    protected void StateHasChanged()
    {
        Changed?.Invoke(this);
    }

    public virtual void Dispose() { }
}