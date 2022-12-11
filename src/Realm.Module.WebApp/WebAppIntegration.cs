using Realm.Interfaces.Grpc;
using Realm.Module.Scripting.Functions;
using Realm.Module.Scripting.Interfaces;

namespace Realm.Module.WebApp;

internal class WebAppIntegration
{
    private readonly EventScriptingFunctions _eventScriptingFunctions;

    public WebAppIntegration(EventScriptingFunctions eventScriptingFunctions, IGrpcDiscord grpcDiscord)
    {
        _eventScriptingFunctions = eventScriptingFunctions;
    }


    public void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {

    }
}
