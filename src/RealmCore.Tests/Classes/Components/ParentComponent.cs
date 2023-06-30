using RealmCore.Server.Components;

namespace RealmCore.Tests.Classes.Components;

internal class ParentComponent : Component
{
    public override void Dispose()
    {
        Entity.TryDestroyComponent<ChildComponent>();
    }
}
