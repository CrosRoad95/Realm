namespace RealmCore.Tests.Classes.Components;

public class TestComponent : ComponentLifecycle
{
    public bool IsDisposed() => Element == null;
}
