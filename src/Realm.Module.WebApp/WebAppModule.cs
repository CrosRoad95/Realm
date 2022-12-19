using Microsoft.Extensions.DependencyInjection;

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
    }
}
