namespace RealmCore.Server.Interfaces;

public interface IECS
{
    IReadOnlyCollection<Entity> Entities { get; }
    IReadOnlyCollection<Entity> VehicleEntities { get; }
    IReadOnlyCollection<Entity> PlayerEntities { get; }
    Entity Console { get; }

    event Action<Entity>? EntityCreated;

    Entity CreateEntity(string name, EntityTag tag, Action<Entity>? entityBuilder = null);
    Entity GetByElement(Element element);
    bool GetVehicleById(int id, out Entity? entity);
    IEnumerable<Entity> GetWithinRange(Vector3 position, float range);
    bool TryGetByElement(Element element, out Entity result);
    bool TryGetEntityByPlayer(Player player, out Entity result, bool ignoreDestroyed = false);
}
