using RealmCore.Module.Web.AdminPanel.Interfaces;

namespace RealmCore.Console.Extra;

internal class WebAdminPanelLogic
{
    public WebAdminPanelLogic(IWebAdminPanelService webAdminPanelService, IElementCollection elementCollection)
    {
        webAdminPanelService.Dashboard.AddTextElement("Ilość graczy", "opis1", () => elementCollection.GetByType<RealmPlayer>().Count().ToString());
        webAdminPanelService.Dashboard.AddTextElement("tytuł2", "opis2", () => $"Jakiś kontent2 {DateTime.Now}");
        webAdminPanelService.Dashboard.AddTextElement("tytuł3", "opis3", () => "Jakiś kontent3");
    }
}
