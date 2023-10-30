namespace RealmCore.ECS.Interfaces;

public interface IComponentLifecycle : IComponent, IDisposable
{
    void Attach();
    void Detach();

    event Action<IComponentLifecycle>? Attached;
    event Action<IComponentLifecycle>? Detached;
}
