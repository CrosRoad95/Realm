using Realm.Scripting.Interfaces;

namespace Realm.WebApp;

public class WebPanelIntegration
{
    private readonly SnackbarFunctions _snackbarFunctions;

    public WebPanelIntegration(SnackbarFunctions snackbarFunctions)
    {
        _snackbarFunctions = snackbarFunctions;
    }

    public void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {
        // Functions
        scriptingModuleInterface.AddHostObject("WebPanelSnackbar", _snackbarFunctions, true);    
    }
}
