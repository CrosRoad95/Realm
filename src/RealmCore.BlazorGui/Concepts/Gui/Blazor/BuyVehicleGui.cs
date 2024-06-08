namespace RealmCore.BlazorGui.Concepts.Gui.Blazor;

public class BuyVehicleGui : BrowserGui
{
    public BuyVehicleGui(RealmPlayer player, string vehicleName, decimal price) : base(player, "/realmUi/buyVehicle", new Dictionary<string, string?>
    {
        ["initialCounter"] = "1337"
    })
    {
    }

    public void Test()
    {

    }
}
