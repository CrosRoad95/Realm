namespace RealmCore.Sample.Components.Gui;

public class SampleState
{
    public int Foo { get; set; }
}

public sealed class TestGui : ReactiveDxGui<SampleState>
{
    private static int _i = 0;
    public TestGui(RealmPlayer player) : base(player, "test", true, new())
    {

    }

    protected override void PreGuiOpen(SampleState state)
    {
        state.Foo = ++_i;
    }
}
