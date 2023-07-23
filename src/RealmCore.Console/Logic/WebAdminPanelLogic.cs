using RealmCore.Module.Web.AdminPanel.Interfaces;

namespace RealmCore.Console.Logic;

internal class WebAdminPanelLogic
{
    public WebAdminPanelLogic(IWebAdminPanelService webAdminPanelService, IECS ecs)
    {
        webAdminPanelService.Dashboard.AddTextElement("Ilość graczy", "opis1", () => ecs.Entities.Where(x => x.Tag == Server.Enums.EntityTag.Player).Count().ToString());
        webAdminPanelService.Dashboard.AddTextElement("tytuł2", "opis2", () => $"Jakiś kontent2 {DateTime.Now}");
        webAdminPanelService.Dashboard.AddTextElement("tytuł3", "opis3", () => "Jakiś kontent3");
    }
}
