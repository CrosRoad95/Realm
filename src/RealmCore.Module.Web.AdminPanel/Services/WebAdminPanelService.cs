using RealmCore.Module.Web.AdminPanel.Services.Tabs;

namespace RealmCore.Module.Web.AdminPanel.Services;

internal class WebAdminPanelService : IWebAdminPanelService
{
    public DashboardTab Dashboard { get; }

    private int _id;
    public WebAdminPanelService()
    {
        Dashboard = new DashboardTab(GetId);
    }

    private int GetId() => _id++;
}
