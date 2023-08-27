using RealmCore.Server.Components;

namespace RealmCore.Tests.Classes.Components;

internal class ParentComponent : Component
{
    protected override void Detached()
    {
        Entity.TryDestroyComponent<ChildComponent>();
    }
}
