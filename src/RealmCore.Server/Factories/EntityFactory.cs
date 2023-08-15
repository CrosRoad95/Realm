using RealmCore.Persistence.Interfaces;
using static Grpc.Core.Metadata;
using Vehicle = SlipeServer.Server.Elements.Vehicle;

namespace RealmCore.Server.Factories;

internal class EntityFactory : IEntityFactory
{
    private readonly IECS _ecs;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IRPGServer _rpgServer;
    private readonly IDateTimeProvider _dateTimeProvider;
    public EntityFactory(IECS ecs, IVehicleRepository vehicleRepository, IRPGServer rpgServer, IDateTimeProvider dateTimeProvider)
    {
        _ecs = ecs;
        _vehicleRepository = vehicleRepository;
        _rpgServer = rpgServer;
        _dateTimeProvider = dateTimeProvider;
    }

    private void AssociateWithServer(Entity entity)
    {
        if (!_rpgServer.IsReady)
            throw new InvalidOperationException("Could not create entities in logics constructors.");
        var elementComponent = entity.GetRequiredComponent<ElementComponent>();

        if (!elementComponent.BaseLoaded)
            throw new Exception("Failed to load element entity, base.Load was not called.");

        var element = elementComponent.Element;
        _rpgServer.AssociateElement(element);
        if (element is Pickup pickup)
        {
            _rpgServer.AssociateElement(pickup.CollisionShape);
        }
        if (elementComponent is MarkerElementComponent markerElementComponent)
        {
            _rpgServer.AssociateElement(markerElementComponent.CollisionShape);
        }
    }

    private void AssociateWithPlayerEntity<TElementComponent>(PlayerPrivateElementComponent<TElementComponent> elementComponent, Entity playerEntity) where TElementComponent : ElementComponent
    {
        var element = elementComponent.Element;
        var player = playerEntity.Player;
        element.AssociateWith(player);
        if (element is Pickup pickup)
        {
            pickup.CollisionShape.AssociateWith(player);
        }

        if (elementComponent.ElementComponent is MarkerElementComponent markerElementComponent)
        {
            markerElementComponent.CollisionShape.AssociateWith(player);
        }
    }

    private void AssociateWithPlayerEntity(ElementComponent elementComponent, Entity playerEntity)
    {
        var element = elementComponent.Element;

        element.Id = (ElementId)playerEntity.GetRequiredComponent<PlayerElementComponent>().MapIdGenerator.GetId();
        var player = playerEntity.Player;
        element.AssociateWith(player);
        if (element is Pickup pickup)
        {
            pickup.CollisionShape.AssociateWith(player);
        }
        if (elementComponent is MarkerElementComponent markerElementComponent)
        {
            markerElementComponent.CollisionShape.AssociateWith(player);
        }
    }

    public Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var vehicleEntity = _ecs.CreateEntity(constructionInfo?.Id ?? $"vehicle {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<VehicleTagComponent>();
            var vehicle = new Vehicle(model, position);

            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle));

            entity.Transform.Position = position;
            vehicle.RespawnPosition = position;
            entity.Transform.Rotation = rotation;
            vehicle.RespawnRotation = rotation;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return vehicleEntity;
    }

    public async Task<Entity> CreateNewPrivateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var vehicleEntity = _ecs.CreateEntity(constructionInfo?.Id ?? $"vehicle {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<VehicleTagComponent>();
            var vehicle = new Vehicle(model, position);
            vehicle.Handling = VehicleHandlingConstants.DefaultVehicleHandling[vehicle.Model];
            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle));

            entity.Transform.Position = position;
            vehicle.RespawnPosition = position;
            entity.Transform.Rotation = rotation;
            vehicle.RespawnRotation = rotation;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        vehicleEntity.AddComponent(new PrivateVehicleComponent(await _vehicleRepository.CreateNewVehicle(model, _dateTimeProvider.Now)));

        return vehicleEntity;
    }

    public Entity CreateMarker(MarkerType markerType, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"marker {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<MarkerTagComponent>();
            var marker = new Marker(new Vector3(0, 0, 1000), markerType)
            {
                Color = Color.White
            };

            var markerElementComponent = entity.AddComponent(new MarkerElementComponent(marker));

            entity.Transform.Position = position;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreatePickup(ushort model, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var pickupEntity = _ecs.CreateEntity(constructionInfo?.Id ?? $"marker {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<PickupTagComponent>();
            entity.AddComponent(new PickupElementComponent(new Pickup(Vector3.Zero, model)));

            entity.Transform.Position = position;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return pickupEntity;
    }

    public Entity CreateBlip(BlipIcon blipIcon, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var blipEntity = _ecs.CreateEntity(constructionInfo?.Id ?? $"marker {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<BlipTagComponent>();
            entity.AddComponent(new BlipElementComponent(new Blip(Vector3.Zero, blipIcon, 250)));

            entity.Transform.Position = position;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return blipEntity;
    }

    public PlayerPrivateElementComponent<BlipElementComponent> CreateBlipFor(Entity playerEntity, BlipIcon blipIcon, Vector3 position)
    {
        if(!playerEntity.HasComponent<PlayerTagComponent>())
            throw new ArgumentException("Entity must be a player entity");

        var blip = new Blip(position, blipIcon, 250);

        var blipElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new BlipElementComponent(blip)));
        AssociateWithPlayerEntity(blipElementComponent, playerEntity);
        return blipElementComponent;
    }

    public Entity CreateRadarArea(Vector2 position, Vector2 size, Color color, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var blipEntity = _ecs.CreateEntity(constructionInfo?.Id ?? $"radarArea {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<RadarAreaTagComponent>();
            entity.AddComponent(new RadarAreaElementComponent(new RadarArea(position, size, color)));

            entity.Transform.Position = new Vector3(position, 0);
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return blipEntity;
    }

    public PlayerPrivateElementComponent<RadarAreaElementComponent> CreateRadarAreaFor(Entity playerEntity, Vector2 position, Vector2 size, Color color)
    {
        if (!playerEntity.HasComponent<PlayerTagComponent>())
            throw new ArgumentException("Entity must be a player entity");

        var radarArea = new RadarArea(position, size, color);

        var radarAreaElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new RadarAreaElementComponent(radarArea)));
        AssociateWithPlayerEntity(radarAreaElementComponent, playerEntity);
        return radarAreaElementComponent;
    }

    public PlayerPrivateElementComponent<CollisionSphereElementComponent> CreateCollisionSphereFor(Entity playerEntity, Vector3 position, float radius)
    {
        if (!playerEntity.HasComponent<PlayerTagComponent>())
            throw new ArgumentException("Entity must be a player entity");

        var collisionSphere = new CollisionSphere(position, radius);

        var collisionSphereElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new CollisionSphereElementComponent(collisionSphere)));
        AssociateWithPlayerEntity(collisionSphereElementComponent, playerEntity);
        return collisionSphereElementComponent;
    }

    public PlayerPrivateElementComponent<MarkerElementComponent> CreateMarkerFor(Entity playerEntity, Vector3 position, MarkerType markerType, Color? color = null)
    {
        if (!playerEntity.HasComponent<PlayerTagComponent>())
            throw new ArgumentException("Entity must be a player entity");

        var marker = new Marker(position, markerType)
        {
            Color = color ?? Color.White
        };

        var markerElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new MarkerElementComponent(marker)));
        AssociateWithPlayerEntity(markerElementComponent, playerEntity);
        return markerElementComponent;
    }

    public Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"object {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<WorldObjectTagComponent>();
            entity.AddComponent(new WorldObjectComponent(new WorldObject(model, position)));

            entity.Transform.Position = position;
            entity.Transform.Rotation = rotation;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public PlayerPrivateElementComponent<WorldObjectComponent> CreateObjectFor(Entity playerEntity, ObjectModel model, Vector3 position, Vector3 rotation)
    {
        if (!playerEntity.HasComponent<PlayerTagComponent>())
            throw new ArgumentException("Entity must be a player entity");

        var worldObject = new WorldObject(model, position)
        {
            Rotation = rotation
        };

        var worldObjectElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new WorldObjectComponent(worldObject)));
        AssociateWithPlayerEntity(worldObjectElementComponent, playerEntity);
        return worldObjectElementComponent;
    }

    public Entity CreateObjectVisibleFor(Entity playerEntity, ObjectModel model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        if (!playerEntity.HasComponent<PlayerTagComponent>())
            throw new ArgumentException("Entity must be a player entity");

        return _ecs.CreateEntity(constructionInfo?.Id ?? $"object {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<WorldObjectTagComponent>();
            var elementComponent = entity.AddComponent(new WorldObjectComponent(new WorldObject(model, position)));

            entity.Transform.Position = position;
            entity.Transform.Rotation = rotation;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithPlayerEntity(elementComponent, playerEntity);
            entityBuilder?.Invoke(entity);
        });
    }

    #region Collision shapes
    public Entity CreateCollisionCircle(Vector2 position, float radius, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision circle {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionCircleElementComponent(new CollisionCircle(new Vector2(0, 0), radius)));

            entity.Transform.Position = new Vector3(position, 0);
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionCuboid(Vector3 position, Vector3 dimensions, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision cuboid {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();
            var collisionCuboid = new CollisionCuboid(position, dimensions);

            entity.Transform.Position = position;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            var collisionCuboidElementComponent = entity.AddComponent(new CollisionCuboidElementComponent(collisionCuboid));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision polygon {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionPolygonElementComponent(new CollisionPolygon(position, vertices)));
            entity.Transform.Position = position;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionRectangle(Vector2 position, Vector2 dimensions, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision rectangle {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionRectangleElementComponent(new CollisionRectangle(position, dimensions)));
            entity.Transform.Position = new Vector3(position, 0);
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionSphere(Vector3 position, float radius, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision sphere {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionSphereElementComponent(new CollisionSphere(new Vector3(0, 0, 1000), radius)));
            entity.Transform.Position = position;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionTube(Vector3 position, float radius, float height, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision tube {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionTubeElementComponent(new CollisionTube(position, radius, height)));
            entity.Transform.Position = position;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }
    
    public Entity CreatePed(PedModel pedModel, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"ped {Guid.NewGuid()}", entity =>
        {
            entity.AddComponent<PedTagComponent>();
            entity.AddComponent(new PedElementComponent(new Ped(pedModel, position)));
            entity.Transform.Position = position;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    #endregion
}
