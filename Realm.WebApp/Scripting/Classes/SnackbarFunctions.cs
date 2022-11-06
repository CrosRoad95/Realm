using Microsoft.ClearScript;

namespace Realm.WebApp.Scripting.Classes;

public class SnackbarFunctions
{
    [NoScriptAccess]
    public event Action<string>? SnackbarAdded;
    public SnackbarFunctions()
    {

    }

    public bool WebPanelAddSnackbar(string message)
    {
        SnackbarAdded?.Invoke(message);
        return true;
    }
}
