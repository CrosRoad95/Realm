using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Console.Components.Gui;

public class SampleState
{
    public int Foo { get; set; }
}

[ComponentUsage(false)]
public sealed class TestWindowComponent : StatefulGuiComponent<SampleState>
{
    private static int i = 0;
    public TestWindowComponent() : base("test", true, new())
    {

    }

    protected override void PreGuiOpen(SampleState state)
    {
        state.Foo = ++i;
    }
}
