using Microsoft.Extensions.DependencyInjection;
using Realm.Module.Scripting.Interfaces;

namespace Realm.Module.WebApp;

public class WebAppModule : IModule
{
    public string Name => "Discord";

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<WebAppIntegration>();
    }

    public void Init(IServiceProvider serviceProvider)
    {
        var discordIntegration = serviceProvider.GetRequiredService<WebAppIntegration>();
    }

    public T GetInterface<T>() where T : class => throw new NotSupportedException();

    public void PostInit(IServiceProvider serviceProvider)
    {
        var discordIntegration = serviceProvider.GetRequiredService<WebAppIntegration>();

        var scriptingModule = serviceProvider.GetRequiredService<IEnumerable<IModule>>().FirstOrDefault(x => x.Name == "Scripting");
        if (scriptingModule != null)
            discordIntegration.InitializeScripting(scriptingModule.GetInterface<IScriptingModuleInterface>());
    }

    public void Reload()
    {

    }

    public int GetPriority() => 100;
}
