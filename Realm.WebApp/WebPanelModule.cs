using Realm.Interfaces.Extend;
using Realm.Scripting.Interfaces;

namespace Realm.WebApp;

public class WebPanelModule : IModule
{
    private readonly WebPanelIntegration _webPanelIntegration;

    public string Name => "WebPanel";

    public WebPanelModule(WebPanelIntegration webPanelIntegration)
    {
        _webPanelIntegration = webPanelIntegration;
    }
    public void Configure(IServiceCollection services)
    {

    }

    public void Init(IServiceProvider serviceProvider)
    {
    }

    public T GetInterface<T>() where T : class => throw new NotSupportedException();

    public void PostInit(IServiceProvider serviceProvider)
    {
        var scriptingModule = serviceProvider.GetRequiredService<IEnumerable<IModule>>().FirstOrDefault(x => x.Name == "Scripting");
        if (scriptingModule != null)
            _webPanelIntegration.InitializeScripting(scriptingModule.GetInterface<IScriptingModuleInterface>());
    }

    public void Reload()
    {

    }

    public int GetPriority() => 200;
}

