namespace RealmCore.Sample.Components.Gui.Blazor;

public class InteractiveGuiComponent : BrowserGui
{
    public string Foo { get; private set; } = "default";
    public event Action? FooChanged;
    public InteractiveGuiComponent(RealmPlayer player) : base(player, "/realmUi/interactive")
    {
    }

    public void SetFoo(string foo)
    {
        Foo = foo;
        FooChanged?.Invoke();
    }
}
