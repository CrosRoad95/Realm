namespace Realm.Server.Factories;

internal class EntityFactory : IEntityFactory
{
    private readonly ECS _ecs;

    public EntityFactory(ECS ecs)
    {
        _ecs = ecs;
    }

    public Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"vehicle {Guid.NewGuid()}", Entity.VehicleTag, entity =>
        {
            var vehicle = new Vehicle(model, new Vector3(0,0,1000));
            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle));

            entity.Transform.Position = position;
            entity.Transform.Rotation = rotation;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            entityBuilder?.Invoke(entity);
        });
    }
}
