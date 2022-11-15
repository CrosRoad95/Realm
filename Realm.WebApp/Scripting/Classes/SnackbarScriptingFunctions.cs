using Microsoft.ClearScript;

namespace Realm.WebApp.Scripting.Classes;

public class SnackbarScriptingFunctions
{
    [NoScriptAccess]
    public event Action<string>? SnackbarAdded;
    public SnackbarScriptingFunctions()
    {

    }

    public bool WebPanelAddSnackbar(string message)
    {
        SnackbarAdded?.Invoke(message);
        return true;
    }
}
