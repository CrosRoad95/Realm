using RealmCore.Module.Web.AdminPanel.Services.Tabs;

namespace RealmCore.Module.Web.AdminPanel.Interfaces;

public interface IWebAdminPanelService
{
    DashboardTab Dashboard { get; }
}
