using RealmCore.ECS.Components;

namespace RealmCore.Tests.Classes.Components;

internal class ParentComponent : Component
{
    protected override void Detach()
    {
        Entity.TryDestroyComponent<ChildComponent>();
    }
}
