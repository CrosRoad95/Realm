using RealmCore.Resources.CEFBlazorGui;

namespace RealmCore.Server.Components.Players;

[ComponentUsage(true)]
public class BlazorGuiComponent : Component
{
    [Inject]
    private ICEFBlazorGuiService CEFBlazorGuiService { get; set; } = default!;

    public event Action<BlazorGuiComponent, string?>? PathChanged;
    internal Action<BlazorGuiComponent, string?, bool, bool>? InternalPathChanged { get; set; }
    internal Action<BlazorGuiComponent, bool?>? InternalDevToolsChanged { get; set; }

    private string? _path;
    public string? Path
    {
        get => _path; set
        {
            if (_path == value) return;
            InternalPathChanged?.Invoke(this, value, false, false);
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
                if(value)
                {
                    CEFBlazorGuiService.SetDevelopmentMode(Entity.Player, value);
                    CEFBlazorGuiService.ToggleDevTools(Entity.Player, value);
                }
                else
                {
                    CEFBlazorGuiService.ToggleDevTools(Entity.Player, value);
                    CEFBlazorGuiService.SetDevelopmentMode(Entity.Player, value);
                }
                _devTools = value;
                InternalDevToolsChanged?.Invoke(this, value);
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
                CEFBlazorGuiService.SetVisible(Entity.Player, value);
                _visible = value;
                InternalDevToolsChanged?.Invoke(this, value);
            }
        }
    }

    public BlazorGuiComponent()
    {

    }

    public void Open(string path, bool force = false, bool isAsync = false)
    {
        if(isAsync)
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

        InternalPathChanged?.Invoke(this, path, force, isAsync);
    }
    
    public void Close()
    {
        Visible = false;
    }

    internal void SetPath(string? path)
    {
        _path = path;
        PathChanged?.Invoke(this, path);
    }
}
