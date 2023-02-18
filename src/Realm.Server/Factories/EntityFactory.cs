using Realm.Domain.Components.CollisionShapes;
using Realm.Persistance.Interfaces;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using Vehicle = SlipeServer.Server.Elements.Vehicle;

namespace Realm.Server.Factories;

internal class EntityFactory : IEntityFactory
{
    private readonly ECS _ecs;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly RPGServer _rpgServer;

    public EntityFactory(ECS ecs, IVehicleRepository vehicleRepository, RPGServer rpgServer)
    {
        _ecs = ecs;
        _vehicleRepository = vehicleRepository;
        _rpgServer = rpgServer;
    }

    private void AssociateWithServer(Entity entity)
    {
        var elementComponent = entity.GetRequiredComponent<ElementComponent>();
        var element = elementComponent.Element;
        element.AssociateWith(_rpgServer.MtaServer);
        if (element is Pickup pickup)
        {
            pickup.CollisionShape.AssociateWith(_rpgServer.MtaServer);
        }
        if(elementComponent is MarkerElementComponent markerElementComponent)
        {
            markerElementComponent.CollisionShape.AssociateWith(_rpgServer.MtaServer);
        }
    }

    private void AssociateWithPlayerEntity(ElementComponent elementComponent, Entity playerEntity)
    {
        var element = elementComponent.Element;
        var player = playerEntity.GetRequiredComponent<PlayerElementComponent>().Player;
        element.AssociateWith(player);
        if(element is Pickup pickup)
        {
            pickup.CollisionShape.AssociateWith(player);
        }
        if (elementComponent is MarkerElementComponent markerElementComponent)
        {
            markerElementComponent.CollisionShape.AssociateWith(_rpgServer.MtaServer);
        }
    }

    public Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        var vehicleEntity = _ecs.CreateEntity(id ?? $"vehicle {Guid.NewGuid()}", Entity.EntityTag.Vehicle, entity =>
        {
            var vehicle = new Vehicle(model, new Vector3(0,0,4));
            
            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle));
            
            entity.Transform.Position = position;
            entity.Transform.Rotation = rotation;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });

        return vehicleEntity;
    }
    
    public async Task<Entity> CreateNewPrivateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        var vehicleEntity = _ecs.CreateEntity(id ?? $"vehicle {Guid.NewGuid()}", Entity.EntityTag.Vehicle, entity =>
        {
            var vehicle = new Vehicle(model, position);

            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle));

            entity.Transform.Position = position;
            entity.Transform.Rotation = rotation;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });

        vehicleEntity.AddComponent(new PrivateVehicleComponent(await _vehicleRepository.CreateNewVehicle(model)));

        return vehicleEntity;
    }

    public Entity CreateMarker(MarkerType markerType, Vector3 position, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"marker {Guid.NewGuid()}", Entity.EntityTag.Marker, entity =>
        {
            var marker = new Marker(new Vector3(0,0,1000), markerType);
            marker.Color = System.Drawing.Color.White;

            var markerElementComponent = entity.AddComponent(new MarkerElementComponent(marker));

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreatePickup(ushort model, Vector3 position, string? id = null)
    {
        var pickupEntity = _ecs.CreateEntity(id ?? $"marker {Guid.NewGuid()}", Entity.EntityTag.Pickup, entity =>
        {
            entity.AddComponent(new PickupElementComponent(new Pickup(Vector3.Zero, model)));

            AssociateWithServer(entity);

            entity.Transform.Position = position;
        });

        return pickupEntity;
    }
    
    public Entity CreateBlip(BlipIcon blipIcon, Vector3 position, string? id = null)
    {
        var blipEntity = _ecs.CreateEntity(id ?? $"marker {Guid.NewGuid()}", Entity.EntityTag.Blip, entity =>
        {
            entity.AddComponent(new BlipElementComponent(new Blip(Vector3.Zero, blipIcon, 250)));

            AssociateWithServer(entity);

            entity.Transform.Position = position;
        });

        return blipEntity;
    }
    
    public BlipElementComponent CreateBlipFor(Entity playerEntity, BlipIcon blipIcon, Vector3 position)
    {
        if(playerEntity.Tag != Entity.EntityTag.Player)
            throw new ArgumentException("Entity must be a player entity");

        var blip = new Blip(position, blipIcon, 250);

        var blipElementComponent = playerEntity.AddComponent(new BlipElementComponent(blip));
        playerEntity.Transform.Position = position;
        AssociateWithPlayerEntity(blipElementComponent, playerEntity);
        return blipElementComponent;
    }

    public CollisionSphereElementComponent CreateCollisionSphereFor(Entity playerEntity, Vector3 position, float radius)
    {
        if (playerEntity.Tag != Entity.EntityTag.Player)
            throw new ArgumentException("Entity must be a player entity");

        var collisionSphere = new CollisionSphere(position, radius);

        var collisionSphereElementComponent = playerEntity.AddComponent(new CollisionSphereElementComponent(collisionSphere));
        playerEntity.Transform.Position = position;
        AssociateWithPlayerEntity(collisionSphereElementComponent, playerEntity);
        return collisionSphereElementComponent;
    }

    public MarkerElementComponent CreateMarkerFor(Entity playerEntity, Vector3 position, MarkerType markerType, System.Drawing.Color? color = null)
    {
        if (playerEntity.Tag != Entity.EntityTag.Player)
            throw new ArgumentException("Entity must be a player entity");

        var marker = new Marker(position, markerType);
        marker.Color = color ?? System.Drawing.Color.White;

        playerEntity.Transform.Position = position;

        var markerElementComponent = playerEntity.AddComponent(new MarkerElementComponent(marker));
        AssociateWithPlayerEntity(markerElementComponent, playerEntity);
        return markerElementComponent;
    }

    public Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"object {Guid.NewGuid()}", Entity.EntityTag.WorldObject, entity =>
        {
            var worldObject = new WorldObject(model, position);

            entity.Transform.Position = position;
            entity.Transform.Rotation = rotation;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var worldObjectComponent = entity.AddComponent(new WorldObjectComponent(worldObject));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }

    #region Collision shapes

    public Entity CreateCollisionCircle(Vector2 position, float radius, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision circle {Guid.NewGuid()}", Entity.EntityTag.CollisionShape, entity =>
        {
            var collisionSphere = new CollisionCircle(new Vector2(0, 0), radius);

            entity.Transform.Position = new Vector3(position, 0);
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var collisionCircleElementComponent = entity.AddComponent(new CollisionCircleElementComponent(collisionSphere));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }
    
    public Entity CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision cuboid {Guid.NewGuid()}", Entity.EntityTag.CollisionShape, entity =>
        {
            var collisioncuboid = new CollisionCuboid(position, dimensions);

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var collisionCuboidElementComponent = entity.AddComponent(new CollisionCuboidElementComponent(collisioncuboid));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }
    
    public Entity CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision polygon {Guid.NewGuid()}", Entity.EntityTag.CollisionShape, entity =>
        {
            var collisionPolygon = new CollisionPolygon(position, vertices);
            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var collisionPolygonElementComponent = entity.AddComponent(new CollisionPolygonElementComponent(collisionPolygon));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision sphere {Guid.NewGuid()}", Entity.EntityTag.CollisionShape, entity =>
        {
            var collisionRectangle = new CollisionRectangle(position, dimensions);

            entity.Transform.Position = new Vector3(position, 0);
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var collisionRectangleElementComponent = entity.AddComponent(new CollisionRectangleElementComponent(collisionRectangle));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }
    
    public Entity CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision sphere {Guid.NewGuid()}", Entity.EntityTag.CollisionShape, entity =>
        {
            var collisionSphere = new CollisionSphere(new Vector3(0, 0, 1000), radius);

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var collisionSphereElementComponent = entity.AddComponent(new CollisionSphereElementComponent(collisionSphere));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }
    
    public Entity CreateCollisionTube(Vector3 position, float radius, float height, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"collision sphere {Guid.NewGuid()}", Entity.EntityTag.CollisionShape, entity =>
        {
            var collisionTube = new CollisionTube(position, radius, height);

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var collisionTubeElementComponent = entity.AddComponent(new CollisionTubeElementComponent(collisionTube));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }
    
    public Entity CreatePed(PedModel pedModel, Vector3 position, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(id ?? $"ped {Guid.NewGuid()}", Entity.EntityTag.Ped, entity =>
        {
            var ped = new Ped(pedModel, position);

            entity.Transform.Position = position;
            entity.Transform.Interior = interior;
            entity.Transform.Dimension = dimension;

            var pedElementComponent = entity.AddComponent(new PedElementComponent(ped));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }

    #endregion
}
