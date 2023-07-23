using RealmCore.Module.Web.AdminPanel.Data;
using RealmCore.Module.Web.AdminPanel.Enums;
using WebAdminPanel;

namespace RealmCore.Module.Web.AdminPanel.Services.Tabs;

public class DashboardTab
{
    private readonly Func<int> _getId;

    internal List<DashboardElementDto> DashboardElements { get; } = new();

    public DashboardTab(Func<int> getId)
    {
        _getId = getId;
    }

    public void AddTextElement(string title, string description, Func<string> contentFactory)
    {
        DashboardElements.Add(new DashboardElementDto
        {
            Id = _getId(),
            Type = DashboardElementType.Text,
            Name = title,
            Description = description,
            DataFactory = contentFactory,
        });
    }
}
