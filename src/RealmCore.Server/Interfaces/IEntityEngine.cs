namespace RealmCore.Server.Interfaces;

public interface IEntityEngine
{
    IReadOnlyCollection<Entity> Entities { get; }
    IEnumerable<Entity> VehicleEntities { get; }
    IEnumerable<Entity> PlayerEntities { get; }
    int EntitiesCount { get; }
    int EntitiesComponentsCount { get; }

    event Action<Entity>? EntityCreated;

    bool ContainsEntity(Entity entity);
    Entity CreateEntity(Action<Entity>? entityBuilder = null);
    IEnumerable<Entity> GetEntitiesContainingComponent<TComponent>() where TComponent : Component;
    bool GetVehicleById(int id, out Entity? entity);
    internal void RemoveEntity(Entity entity);
}
