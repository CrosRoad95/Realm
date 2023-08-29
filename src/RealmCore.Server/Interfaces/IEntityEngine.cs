namespace RealmCore.Server.Interfaces;

public interface IEntityEngine
{
    IReadOnlyCollection<Entity> Entities { get; }
    IEnumerable<Entity> VehicleEntities { get; }
    IEnumerable<Entity> PlayerEntities { get; }
    Entity Console { get; }
    int EntitiesCount { get; }
    int EntitiesComponentsCount { get; }

    event Action<Entity>? EntityCreated;

    bool ContainsEntity(Entity entity);
    Entity CreateEntity(string name, Action<Entity>? entityBuilder = null);
    Entity GetByElement(Element element);
    IEnumerable<Entity> GetEntitiesContainingComponent<TComponent>() where TComponent : Component;
    bool GetVehicleById(int id, out Entity entity);
    IEnumerable<Entity> GetWithinRange(Vector3 position, float range);
    bool TryGetByElement(Element element, out Entity result);
    bool TryGetEntityByPed(Ped ped, out Entity result, bool ignoreDestroyed = false);
    bool TryGetEntityByPlayer(Player player, out Entity result, bool ignoreDestroyed = false);

    internal void RemoveEntity(Entity entity);
}
