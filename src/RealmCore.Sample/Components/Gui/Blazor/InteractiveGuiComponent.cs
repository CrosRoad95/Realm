using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Sample.Components.Gui.Blazor;

public class InteractiveGuiComponent : BrowserGuiComponent
{
    public string Foo { get; private set; } = "default";
    public event Action? FooChanged;
    public InteractiveGuiComponent() : base("/realmUi/interactive")
    {
    }

    public void SetFoo(string foo)
    {
        Foo = foo;
        FooChanged?.Invoke();
    }
}
