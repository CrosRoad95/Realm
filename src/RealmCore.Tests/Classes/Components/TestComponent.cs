namespace RealmCore.Tests.Classes.Components;

public class TestComponent : Component
{
    public bool IsDisposed()
    {
        try
        {
            ThrowIfDisposed();
            return false;
        }
        catch
        {
            return true;
        }
    }
}
