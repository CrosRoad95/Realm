
namespace RealmCore.Server.Components;

public interface IComponents
{
    Concepts.Components Components { get; }

    TComponent AddComponent<TComponent>() where TComponent : IComponent, new();
    TComponent AddComponent<TComponent>(TComponent component) where TComponent : IComponent;
    void DestroyComponent<TComponent>() where TComponent : IComponent;
    void DestroyComponent<TComponent>(TComponent component) where TComponent : IComponent;
    TComponent GetRequiredComponent<TComponent>() where TComponent : IComponent;
    bool HasComponent<TComponent>() where TComponent : IComponent;
    bool TryDestroyComponent<TComponent>() where TComponent : IComponent;
    bool TryGetComponent<TComponent>(out TComponent component) where TComponent : IComponent;
}
