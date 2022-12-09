using Microsoft.ClearScript;

namespace Realm.WebApp.Scripting.Classes;

[NoDefaultScriptAccess]
public class SnackbarScriptingFunctions
{
    public event Action<string>? SnackbarAdded;
    public SnackbarScriptingFunctions()
    {

    }

    [ScriptMember("webPanelAddSnackbar")]
    public bool WebPanelAddSnackbar(string message)
    {
        SnackbarAdded?.Invoke(message);
        return true;
    }
}
