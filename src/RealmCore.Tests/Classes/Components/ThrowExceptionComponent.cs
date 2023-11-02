namespace RealmCore.Tests.Classes.Components;

public class ThrowExceptionComponent : ComponentLifecycle
{
    public override void Attach()
    {
        throw new Exception("Something went wrong");
    }
}
