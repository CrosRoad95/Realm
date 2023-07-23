using WebAdminPanel;

namespace RealmCore.Module.Web.AdminPanel;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebAdminPanelModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<WebAdminPanelDashboardStub>();
        serviceCollection.AddSingleton(x => Dashboard.BindService(x.GetRequiredService<WebAdminPanelDashboardStub>()));

        serviceCollection.AddSingleton<IWebAdminPanelService, WebAdminPanelService>();
        serviceCollection.AddSingleton<IModule, WebAdminPanelIntegration>();
        return serviceCollection;
    }
}
