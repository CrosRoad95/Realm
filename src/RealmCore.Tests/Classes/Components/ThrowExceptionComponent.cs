using RealmCore.ECS.Components;

namespace RealmCore.Tests.Classes.Components;

public class ThrowExceptionComponent : Component
{
    protected override void Attach()
    {
        throw new Exception("Something went wrong");
    }
}
