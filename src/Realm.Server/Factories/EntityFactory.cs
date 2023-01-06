using Realm.Domain.Components.Vehicles;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.IdGeneration;

namespace Realm.Server.Factories;

internal class EntityFactory : IEntityFactory
{
    private readonly ECS _ecs;
    private readonly IElementIdGenerator _elementIdGenerator;

    public EntityFactory(ECS ecs, IElementIdGenerator elementIdGenerator)
    {
        _ecs = ecs;
        _elementIdGenerator = elementIdGenerator;
    }

    public Entity CreateBlipFor(Entity createForEntity, BlipIcon blipIcon, Vector3 position, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"vehicle {Guid.NewGuid()}", Entity.BlipTag, entity =>
        {
            var blip = new Blip(new Vector3(0,0,1000), blipIcon);
            blip.Id = _elementIdGenerator.GetId();
            blip.VisibleDistance = 250;
            var vehicleElementComponent = entity.AddComponent(new BlipElementComponent(blip, createForEntity));

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            entityBuilder?.Invoke(entity);
        });
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

    public Entity CreateMarkerFor(Entity createForEntity, MarkerType markerType, Vector3 position, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        if (createForEntity.Tag != Entity.PlayerTag)
            throw new Exception("Entity must be player entity.");

        return _ecs.CreateEntity(id ?? $"marker {Guid.NewGuid()}", Entity.MarkerTag, entity =>
        {
            var marker = new Marker(new Vector3(0,0,1000), markerType);
            marker.Id = _elementIdGenerator.GetId();
            marker.Color = System.Drawing.Color.White;
            var markerElementComponent = entity.AddComponent(new MarkerElementComponent(marker, createForEntity));

            entity.Transform.Position = position;
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

    public Entity CreateCollisionSphereFor(Entity createForEntity, Vector3 position, float radius, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        if (createForEntity.Tag != Entity.PlayerTag)
            throw new Exception("Entity must be player entity.");

        return _ecs.CreateEntity(id ?? $"collision sphere {Guid.NewGuid()}", Entity.CollisionShape, entity =>
        {
            var collisionSphere = new CollisionSphere(new Vector3(0,0,1000), radius);
            collisionSphere.Id = _elementIdGenerator.GetId();
            var markerElementComponent = entity.AddComponent(new CollisionSphereElementComponent(collisionSphere, createForEntity));

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            entityBuilder?.Invoke(entity);
        });
    }
}
