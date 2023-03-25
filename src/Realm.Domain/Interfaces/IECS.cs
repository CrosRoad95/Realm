namespace Realm.Domain.Interfaces;

public interface IECS
{
    IReadOnlyCollection<Entity> VehicleEntities { get; }
    IReadOnlyCollection<Entity> PlayerEntities { get; }

    event Action<Entity>? EntityCreated;

    Task<AsyncEntity> CreateAsyncEntity(string name, EntityTag tag, Func<AsyncEntity, Task>? entityBuilder = null);
    Entity CreateEntity(string name, EntityTag tag, Action<Entity>? entityBuilder = null);
    Entity GetByElement(Element element);
    bool TryGetByElement(Element element, out Entity result);
    bool TryGetEntityByPlayer(Player player, out Entity result);
}
