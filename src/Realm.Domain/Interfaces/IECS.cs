namespace Realm.Domain.Interfaces;

public interface IECS
{
    IReadOnlyCollection<Entity> Entities { get; }
    IReadOnlyCollection<Entity> VehicleEntities { get; }
    IReadOnlyCollection<Entity> PlayerEntities { get; }

    event Action<Entity>? EntityCreated;

    Entity CreateEntity(string name, EntityTag tag, Action<Entity>? entityBuilder = null);
    Entity GetByElement(Element element);
    bool TryGetByElement(Element element, out Entity result);
    bool TryGetEntityByPlayer(Player player, out Entity result);
}
