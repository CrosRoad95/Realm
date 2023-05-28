using RealmCore.Resources.CEFBlazorGui;

namespace RealmCore.Server.Components.Players;

public class BlazorGuiComponent : Component
{
    [Inject]
    private ICEFBlazorGuiService CEFBlazorGuiService { get; set; } = default!;

    public event Action<BlazorGuiComponent, string?>? PathChanged;
    internal event Action<BlazorGuiComponent, string?>? InternalPathChanged;
    internal event Action<BlazorGuiComponent, bool?>? InternalDevToolsChanged;

    private string? _path;
    public string? Path
    {
        get => _path; set
        {
            if (_path == value) return;
            InternalPathChanged?.Invoke(this, value);
            Visible = true;
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
                CEFBlazorGuiService.SetDevelopmentMode(Entity.Player, value);
                CEFBlazorGuiService.ToggleDevTools(Entity.Player, value);
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

    internal void SetPath(string? path)
    {
        _path = path;
        PathChanged?.Invoke(this, path);
    }
}
