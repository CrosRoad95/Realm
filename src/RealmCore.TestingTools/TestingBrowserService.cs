namespace RealmCore.TestingTools;

public class TestingBrowserService : IBrowserService
{
    public Action<IMessage>? MessageHandler { get; set; }

    public event Action<Player>? BrowserStarted;
    public event Action<Player>? BrowserStopped;
    public event Action<Player>? BrowserLoaded;

    public void RelayBrowserReady(Player player)
    {

    }

    public void RelayBrowserStarted(Player player)
    {

    }

    public void SetPath(Player player, string path)
    {

    }

    public void SetVisible(Player player, bool visible)
    {

    }

    public void ToggleDevTools(Player player, bool enabled)
    {

    }
}
