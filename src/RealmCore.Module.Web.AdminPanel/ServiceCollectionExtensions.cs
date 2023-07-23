using Microsoft.Extensions.DependencyInjection;
using RealmCore.Interfaces.Extend;

namespace RealmCore.Module.Web.AdminPanel;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebAdminPanelModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IModule, WebAdminPanelIntegration>();
        return serviceCollection;
    }
}
