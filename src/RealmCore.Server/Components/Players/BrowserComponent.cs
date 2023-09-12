namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class BrowserComponent : Component
{
    public event Action<BrowserComponent, string?, bool, GuiPageType, GuiPageChangeSource>? PathChanged;
    public event Action<BrowserComponent, string>? RemotePathChanged;
    public event Action<BrowserComponent, bool>? DevToolsStateChanged;
    public event Action<BrowserComponent, bool>? VisibleChanged;

    private string? _path;
    public string? Path
    {
        get => _path; set
        {
            if (_path == value) return;
            PathChanged?.Invoke(this, value, false, GuiPageType.Unknown, GuiPageChangeSource.Server);
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

    public void LoadRemotePage(string path, bool andOpen = true)
    {
        if(andOpen)
            Visible = true;
        RemotePathChanged?.Invoke(this, path);
    }

    public void Open(string path, bool force = false, bool isAsync = false)
    {
        if (isAsync)
        {
            //Visible = true;
            _visible = true;
        }
        else
        {
            Visible = true;
        }

        if (path == _path && !force)
            return;
        _path = path;

        PathChanged?.Invoke(this, path, force, isAsync ? GuiPageType.Async : GuiPageType.Sync, GuiPageChangeSource.Server);
    }

    public void Close()
    {
        Visible = false;
    }

    internal void InternalSetPath(string path)
    {
        _path = path;
        PathChanged?.Invoke(this, path, false, GuiPageType.Unknown, GuiPageChangeSource.Client);
    }
}
