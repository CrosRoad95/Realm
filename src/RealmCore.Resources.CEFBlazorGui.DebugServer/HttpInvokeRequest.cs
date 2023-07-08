namespace RealmCore.Resources.CEFBlazorGui.DebugServer;

internal class HttpInvokeRequest
{
    public string Kind { get; set; }
    public string CSharpIdentifier { get; set; }
    public string Args { get; set; }
}
