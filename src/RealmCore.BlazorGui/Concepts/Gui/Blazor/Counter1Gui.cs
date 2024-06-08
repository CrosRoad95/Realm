namespace RealmCore.BlazorGui.Concepts.Gui.Blazor;

public class Counter1Gui : BrowserGui
{
    public Counter1Gui(RealmPlayer player) : base(player, "/realmUi/counter1B", new Dictionary<string, string?>
    {
        ["initialCounter"] = "1337"
    })
    {
    }

    public void Test()
    {

    }
}
