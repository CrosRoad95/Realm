using Realm.Scripting.Interfaces;
using Realm.Tests.Modules.Scripting.Classes;
using Realm.WebApp;

namespace Realm.Tests.Modules;

internal class TestWebPanelIntegration : WebPanelIntegration
{
    public TestWebPanelIntegration() : base(new TestSnackbarFunctions())
    {
    }

    public new void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {
        ;
    }
}
