namespace RealmCore.Sample.Components.Gui.Blazor;

public class Counter2GuiComponent : BrowserGuiComponent
{
    public Counter2GuiComponent() : base("/realmUi/counter2component")
    {
    }
    public void Test()
    {
        var en = Entity;
    }
}