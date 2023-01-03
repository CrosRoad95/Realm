using RenderWareIo.Structs.Dff;
using SlipeServer.Server.Elements.ColShapes;

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

    public Entity CreateMarker(MarkerType markerType, Vector3 position, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"marker {Guid.NewGuid()}", Entity.MarkerTag, entity =>
        {
            var marker = new Marker(new Vector3(0,0,1000), markerType);
            marker.Color = System.Drawing.Color.White;
            var markerElementComponent = entity.AddComponent(new MarkerElementComponent(marker));

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision sphere {Guid.NewGuid()}", Entity.CollisionShape, entity =>
        {
            var collisionSphere = new CollisionSphere(new Vector3(0,0,1000), radius);
            var markerElementComponent = entity.AddComponent(new CollisionSphereElementComponent(collisionSphere));

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            entityBuilder?.Invoke(entity);
        });
    }
}
