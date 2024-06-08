namespace RealmCore.BlazorGui.Concepts.Gui.Blazor;

public class InteractiveGui : BrowserGui
{
    public string Foo { get; private set; } = "default";
    public event Action? FooChanged;
    public InteractiveGui(RealmPlayer player) : base(player, "/realmUi/interactive")
    {
    }

    public void SetFoo(string foo)
    {
        Foo = foo;
        FooChanged?.Invoke();
    }
}
