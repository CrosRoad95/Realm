using Realm.Scripting.Interfaces;

namespace Realm.WebApp;

public class WebPanelIntegration
{
    private readonly SnackbarScriptingFunctions _snackbarFunctions;

    public WebPanelIntegration(SnackbarScriptingFunctions snackbarFunctions)
    {
        _snackbarFunctions = snackbarFunctions;
    }

    public void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {
        // Functions
        scriptingModuleInterface.AddHostObject("WebPanelSnackbar", _snackbarFunctions, true);    
    }
}
