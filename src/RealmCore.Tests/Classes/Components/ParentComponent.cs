namespace RealmCore.Tests.Classes.Components;

internal class ParentComponent : ComponentLifecycle
{
    public override void Detach()
    {
        ((IComponents)Element).TryDestroyComponent<ChildComponent>();
    }
}
