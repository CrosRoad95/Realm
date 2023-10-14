namespace RealmCore.Resources.Browser;

public class BrowserOptions
{
    public BrowserMode Mode { get; set; }
    public int BrowserWidth { get; set; }
    public int BrowserHeight { get; set; }
    public bool DebuggingServer { get; set; }
    public string? BaseRemoteUrl { get; set; }
    public string? RequestWhitelistUrl { get; set; }
}
