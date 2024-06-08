namespace RealmCore.BlazorGui.Concepts.Gui.Blazor;

public class TestGui : BrowserGui
{
    public TestGui(RealmPlayer player) : base(player, "/realmUi/buyVehicle", new Dictionary<string, string?>
    {
        ["initialCounter"] = "1337"
    })
    {
    }

    public void Test()
    {

    }
}
