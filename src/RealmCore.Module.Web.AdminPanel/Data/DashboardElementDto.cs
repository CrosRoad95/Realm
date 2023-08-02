using RealmCore.Module.Web.AdminPanel.Enums;

namespace RealmCore.Module.Web.AdminPanel.Data;

internal class DashboardElementDto
{
    public int Id { get; set; }
    public DashboardElementType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Func<string> DataFactory { get; set; }
}
