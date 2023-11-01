namespace RealmCore.Sample.Components.Gui.Blazor;

public class Counter1GuiComponent : BrowserGuiComponent
{
    public Counter1GuiComponent() : base("/realmUi/counter1component")
    {
    }

    public void Test()
    {
        var en = Element;
    }
}