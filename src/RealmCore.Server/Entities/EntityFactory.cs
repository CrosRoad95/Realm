using RealmCore.Persistence.Interfaces;
using RealmCore.Server.Components.Elements.Abstractions;
using Vehicle = SlipeServer.Server.Elements.Vehicle;

namespace RealmCore.Server.Entities;

internal sealed class EntityFactory : IEntityFactory
{
    private readonly IEntityEngine _entityEngine;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IRealmServer _realmServer;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IElementCollection _elementCollection;

    public EntityFactory(IEntityEngine entityEngine, IVehicleRepository vehicleRepository, IRealmServer realmServer, IDateTimeProvider dateTimeProvider, IElementCollection elementCollection)
    {
        _entityEngine = entityEngine;
        _vehicleRepository = vehicleRepository;
        _realmServer = realmServer;
        _dateTimeProvider = dateTimeProvider;
        _elementCollection = elementCollection;
    }

    public IScopedEntityFactory CreateScopedEntityFactory(Entity entity)
    {
        return new ScopedEntityFactory(entity, _elementCollection, this, _entityEngine);
    }

    private void AssociateWithServer(Entity entity)
    {
        //if (!_realmServer.IsReady)
        //    throw new InvalidOperationException("Could not create entities in logics constructors.");
        var elementComponent = entity.GetRequiredComponent<ElementComponent>();

        if (!elementComponent.BaseLoaded)
            throw new Exception("Failed to load element entity, base.Load was not called.");

        var element = elementComponent.Element;
        _realmServer.AssociateElement(element);
        if (element is Pickup pickup)
        {
            _realmServer.AssociateElement(pickup.CollisionShape);
        }
        if (elementComponent is MarkerElementComponent markerElementComponent)
        {
            _realmServer.AssociateElement(markerElementComponent.CollisionShape);
        }
    }

    private void AssociateWithPlayerEntity<TElementComponent>(PlayerPrivateElementComponent<TElementComponent> elementComponent, Entity playerEntity) where TElementComponent : ElementComponent
    {
        var element = elementComponent.Element;
        var player = playerEntity.GetPlayer();
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
        var player = playerEntity.GetPlayer();
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
        var vehicleEntity = _entityEngine.CreateEntity(constructionInfo?.Id ?? $"vehicle {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(position, rotation, constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(position, rotation));

            entity.AddComponent<VehicleTagComponent>();
            var vehicle = new Vehicle(model, position);

            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle, _entityEngine));

            vehicle.RespawnPosition = position;
            vehicle.RespawnRotation = rotation;

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return vehicleEntity;
    }

    public async Task<Entity> CreateNewPrivateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var vehicleEntity = _entityEngine.CreateEntity(constructionInfo?.Id ?? $"vehicle {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(position, constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(position));

            entity.AddComponent<VehicleTagComponent>();
            var vehicle = new Vehicle(model, position);
            vehicle.Handling = VehicleHandlingConstants.DefaultVehicleHandling[vehicle.Model];
            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle, _entityEngine));

            vehicle.RespawnPosition = position;
            vehicle.RespawnRotation = rotation;

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        vehicleEntity.AddComponent(new PrivateVehicleComponent(await _vehicleRepository.CreateNewVehicle(model, _dateTimeProvider.Now)));

        return vehicleEntity;
    }

    public Entity CreateMarker(MarkerType markerType, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(constructionInfo?.Id ?? $"marker {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(position, constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(position));

            entity.AddComponent<MarkerTagComponent>();
            var marker = new Marker(new Vector3(0, 0, 1000), markerType)
            {
                Color = Color.White
            };

            var markerElementComponent = entity.AddComponent(new MarkerElementComponent(marker, _elementCollection, _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreatePickup(ushort model, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var pickupEntity = _entityEngine.CreateEntity(constructionInfo?.Id ?? $"marker {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(position, constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(position));

            entity.AddComponent<PickupTagComponent>();
            entity.AddComponent(new PickupElementComponent(new Pickup(Vector3.Zero, model), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return pickupEntity;
    }

    public Entity CreateBlip(BlipIcon blipIcon, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var blipEntity = _entityEngine.CreateEntity(constructionInfo?.Id ?? $"marker {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(position, constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(position));

            entity.AddComponent<BlipTagComponent>();
            entity.AddComponent(new BlipElementComponent(new Blip(Vector3.Zero, blipIcon, 250)));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return blipEntity;
    }

    public Entity CreateRadarArea(Vector2 position, Vector2 size, Color color, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var blipEntity = _entityEngine.CreateEntity(constructionInfo?.Id ?? $"radarArea {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(new Vector3(position, 0), constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(new Vector3(position, 0)));

            entity.AddComponent<RadarAreaTagComponent>();
            entity.AddComponent(new RadarAreaElementComponent(new RadarArea(position, size, color)));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return blipEntity;
    }

    public Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(constructionInfo?.Id ?? $"object {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(position, rotation, constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(position, rotation));

            entity.AddComponent<WorldObjectTagComponent>();
            entity.AddComponent(new WorldObjectComponent(new WorldObject(model, position)));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    #region Collision shapes
    public Entity CreateCollisionCircle(Vector2 position, float radius, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(constructionInfo?.Id ?? $"collision circle {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(new Vector3(position, 0), constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(new Vector3(position, 0)));

            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionCircleElementComponent(new CollisionCircle(new Vector2(0, 0), radius), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionCuboid(Vector3 position, Vector3 dimensions, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(constructionInfo?.Id ?? $"collision cuboid {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(position, constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(position));

            entity.AddComponent<CollisionShapeTagComponent>();
            var collisionCuboid = new CollisionCuboid(position, dimensions);

            var collisionCuboidElementComponent = entity.AddComponent(new CollisionCuboidElementComponent(collisionCuboid, _entityEngine));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(constructionInfo?.Id ?? $"collision polygon {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(position, constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(position));

            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionPolygonElementComponent(new CollisionPolygon(position, vertices), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionRectangle(Vector2 position, Vector2 dimensions, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(constructionInfo?.Id ?? $"collision rectangle {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(new Vector3(position, 0), constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(new Vector3(position, 0)));

            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionRectangleElementComponent(new CollisionRectangle(position, dimensions), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionSphere(Vector3 position, float radius, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(constructionInfo?.Id ?? $"collision sphere {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(position, constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(position));

            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionSphereElementComponent(new CollisionSphere(new Vector3(0, 0, 1000), radius), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionTube(Vector3 position, float radius, float height, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(constructionInfo?.Id ?? $"collision tube {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(position, constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(position));

            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionTubeElementComponent(new CollisionTube(position, radius, height), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreatePed(PedModel pedModel, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(constructionInfo?.Id ?? $"ped {Guid.NewGuid()}", entity =>
        {
            if (constructionInfo != null)
                entity.AddComponent(new Transform(position, constructionInfo.Interior, constructionInfo.Dimension));
            else
                entity.AddComponent(new Transform(position));

            entity.AddComponent<PedTagComponent>();
            entity.AddComponent(new PedElementComponent(new Ped(pedModel, position)));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    #endregion
}
