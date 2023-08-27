using System.Drawing;

namespace RealmCore.Resources.CEFBlazorGui;

public class BlazorOptions
{
    public CEFGuiBlazorMode Mode { get; set; }
    public Size BrowserSize { get; set; }
    public bool DebuggingServer { get; set; }
    public string? BaseRemoteUrl { get; set; }
    public string? RequestWhitelistUrl { get; set; }
}
