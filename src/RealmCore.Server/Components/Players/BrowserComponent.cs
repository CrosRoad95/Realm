namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class BrowserComponent : Component
{
    public event Action<BrowserComponent, string, bool>? PathChanged;
    public event Action<BrowserComponent, bool>? DevToolsStateChanged;
    public event Action<BrowserComponent, bool>? VisibleChanged;

    private string _path = "/";
    public string Path
    {
        get => _path; set
        {
            PathChanged?.Invoke(this, value, false);
            _path = value;
        }
    }

    private bool _devTools;
    public bool DevTools
    {
        get => _devTools; set
        {
            if (_devTools != value)
            {
                _devTools = value;
                DevToolsStateChanged?.Invoke(this, value);
            }
        }
    }

    private bool _visible;
    public bool Visible
    {
        get => _visible; set
        {
            if (_visible != value)
            {
                _visible = value;
                VisibleChanged?.Invoke(this, value);
            }
        }
    }

    public void SetPath(string path, bool clientSide = true)
    {
        PathChanged?.Invoke(this, path, clientSide);
        _path = path;
    }

    public void Close()
    {
        Visible = false;
    }
}
