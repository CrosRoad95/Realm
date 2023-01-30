using Realm.Domain;
using Realm.Domain.Components.CollisionShapes;
using Realm.Domain.Interfaces;
using Realm.Persistance.Interfaces;
using RenderWareIo.Structs.Dff;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.IdGeneration;
using SlipeServer.Server.Enums;
using static Grpc.Core.Metadata;

namespace Realm.Server.Factories;

internal class EntityFactory : IEntityFactory
{
    private readonly ECS _ecs;
    private readonly IVehicleRepository _vehicleRepository;

    public EntityFactory(ECS ecs, IVehicleRepository vehicleRepository)
    {
        _ecs = ecs;
        _vehicleRepository = vehicleRepository;
    }

    public Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"vehicle {Guid.NewGuid()}", Entity.VehicleTag, entity =>
        {
            var vehicle = new SlipeServer.Server.Elements.Vehicle(model, position);

            entity.Transform.Position = position;
            entity.Transform.Rotation = rotation;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle));

            entityBuilder?.Invoke(entity);
        });
    }
    
    public async Task<Entity> CreateNewPrivateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        var vehicleEntity = _ecs.CreateEntity(id ?? $"vehicle {Guid.NewGuid()}", Entity.VehicleTag, entity =>
        {
            var vehicle = new SlipeServer.Server.Elements.Vehicle(model, position);
            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle));

            entityBuilder?.Invoke(entity);
        });

        vehicleEntity.Transform.Position = position;
        vehicleEntity.Transform.Rotation = rotation;
        vehicleEntity.Transform.Interior = interior;
        vehicleEntity.Transform.Dimension = dimension;

        vehicleEntity.AddComponent(new PrivateVehicleComponent(await _vehicleRepository.CreateNewVehicle(model)));

        return vehicleEntity;
    }

    public Entity CreateMarker(MarkerType markerType, Vector3 position, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"marker {Guid.NewGuid()}", Entity.MarkerTag, entity =>
        {
            var marker = new Marker(new Vector3(0,0,1000), markerType);
            marker.Color = System.Drawing.Color.White;

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var markerElementComponent = entity.AddComponent(new MarkerElementComponent(marker));

            entityBuilder?.Invoke(entity);
        });
    }

    public BlipElementComponent CreateBlipFor(Entity entity, BlipIcon blipIcon, Vector3 position)
    {
        if(entity.Tag != Entity.PlayerTag)
            throw new ArgumentException("Entity must be a player entity");

        var blip = new Blip(position, blipIcon, 250);

        entity.Transform.Position = position;

        var blipElementComponent = entity.AddComponent(new BlipElementComponent(blip));
        return blipElementComponent;
    }

    public CollisionSphereElementComponent CreateCollisionSphereFor(Entity entity, Vector3 position, float radius)
    {
        if (entity.Tag != Entity.PlayerTag)
            throw new ArgumentException("Entity must be a player entity");

        var collisionSphere = new CollisionSphere(position, radius);

        var collisionSphereElementComponent = entity.AddComponent(new CollisionSphereElementComponent(collisionSphere));

        entity.Transform.Position = position;
        return collisionSphereElementComponent;
    }

    public MarkerElementComponent CreateMarkerFor(Entity entity, Vector3 position, MarkerType markerType, System.Drawing.Color? color = null)
    {
        if (entity.Tag != Entity.PlayerTag)
            throw new ArgumentException("Entity must be a player entity");

        var marker = new Marker(position, markerType);
        marker.Color = color ?? System.Drawing.Color.White;

        entity.Transform.Position = position;

        var markerElementComponent = entity.AddComponent(new MarkerElementComponent(marker));
        return markerElementComponent;
    }

    public Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"object {Guid.NewGuid()}", Entity.WorldObject, entity =>
        {
            var worldObject = new WorldObject(model, position);

            entity.Transform.Position = position;
            entity.Transform.Rotation = rotation;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var markerElementComponent = entity.AddComponent(new WorldObjectComponent(worldObject));

            entityBuilder?.Invoke(entity);
        });
    }

    #region Collision shapes

    public Entity CreateCollisionCircle(Vector2 position, float radius, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision circle {Guid.NewGuid()}", Entity.CollisionShape, entity =>
        {
            var collisionSphere = new CollisionCircle(new Vector2(0, 0), radius);

            entity.Transform.Position = new Vector3(position, 0);
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var markerElementComponent = entity.AddComponent(new CollisionCircleElementComponent(collisionSphere));

            entityBuilder?.Invoke(entity);
        });
    }
    
    public Entity CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision cuboid {Guid.NewGuid()}", Entity.CollisionShape, entity =>
        {
            var collisioncuboid = new CollisionCuboid(position, dimensions);

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var markerElementComponent = entity.AddComponent(new CollisionCuboidElementComponent(collisioncuboid));

            entityBuilder?.Invoke(entity);
        });
    }
    
    public Entity CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision polygon {Guid.NewGuid()}", Entity.CollisionShape, entity =>
        {
            var collisionPolygon = new CollisionPolygon(position, vertices);
            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var markerElementComponent = entity.AddComponent(new CollisionPolygonElementComponent(collisionPolygon));

            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision sphere {Guid.NewGuid()}", Entity.CollisionShape, entity =>
        {
            var collisionRectangle = new CollisionRectangle(position, dimensions);

            entity.Transform.Position = new Vector3(position, 0);
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var markerElementComponent = entity.AddComponent(new CollisionRectangleElementComponent(collisionRectangle));

            entityBuilder?.Invoke(entity);
        });
    }
    
    public Entity CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision sphere {Guid.NewGuid()}", Entity.CollisionShape, entity =>
        {
            var collisionSphere = new CollisionSphere(new Vector3(0, 0, 1000), radius);

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var markerElementComponent = entity.AddComponent(new CollisionSphereElementComponent(collisionSphere));

            entityBuilder?.Invoke(entity);
        });
    }
    
    public Entity CreateCollisionTube(Vector3 position, float radius, float height, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision sphere {Guid.NewGuid()}", Entity.CollisionShape, entity =>
        {
            var collisionTube = new CollisionTube(position, radius, height);

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var markerElementComponent = entity.AddComponent(new CollisionTubeElementComponent(collisionTube));

            entityBuilder?.Invoke(entity);
        });
    }

    #endregion
}
