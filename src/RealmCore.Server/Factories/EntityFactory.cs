using Vehicle = SlipeServer.Server.Elements.Vehicle;

namespace RealmCore.Server.Factories;

internal class EntityFactory : IEntityFactory
{
    private readonly IECS _ecs;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly RPGServer _rpgServer;
    private readonly IDateTimeProvider _dateTimeProvider;

    public EntityFactory(IECS ecs, IVehicleRepository vehicleRepository, RPGServer rpgServer, IDateTimeProvider dateTimeProvider)
    {
        _ecs = ecs;
        _vehicleRepository = vehicleRepository;
        _rpgServer = rpgServer;
        _dateTimeProvider = dateTimeProvider;
    }

    private void AssociateWithServer(Entity entity)
    {
        if (_rpgServer.MtaServer == null)
            throw new InvalidOperationException("Could not create entities in logics constructors.");
        var elementComponent = entity.GetRequiredComponent<ElementComponent>();

        if (!elementComponent.BaseLoaded)
            throw new Exception("Failed to load element entity, base.Load wans't called.");

        var element = elementComponent.Element;
        element.AssociateWith(_rpgServer.MtaServer);
        if (element is Pickup pickup)
        {
            pickup.CollisionShape.AssociateWith(_rpgServer.MtaServer);
        }
        if (elementComponent is MarkerElementComponent markerElementComponent)
        {
            markerElementComponent.CollisionShape.AssociateWith(_rpgServer.MtaServer);
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
        if (elementComponent is MarkerElementComponent markerElementComponent)
        {
            markerElementComponent.CollisionShape.AssociateWith(player);
        }
    }

    private void AssociateWithPlayerEntity(ElementComponent elementComponent, Entity playerEntity)
    {
        var element = elementComponent.Element;
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
        var vehicleEntity = _ecs.CreateEntity(constructionInfo?.Id ?? $"vehicle {Guid.NewGuid()}", EntityTag.Vehicle, entity =>
        {
            var vehicle = new Vehicle(model, new Vector3(0, 0, 9999));

            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle));

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

        return vehicleEntity;
    }

    public async Task<Entity> CreateNewPrivateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var vehicleEntity = _ecs.CreateEntity(constructionInfo?.Id ?? $"vehicle {Guid.NewGuid()}", EntityTag.Vehicle, entity =>
        {
            var vehicle = new Vehicle(model, position);

            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle));

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

        vehicleEntity.AddComponent(new PrivateVehicleComponent(await _vehicleRepository.CreateNewVehicle(model, _dateTimeProvider.Now)));

        return vehicleEntity;
    }

    public Entity CreateMarker(MarkerType markerType, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"marker {Guid.NewGuid()}", EntityTag.Marker, entity =>
        {
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
        var pickupEntity = _ecs.CreateEntity(constructionInfo?.Id ?? $"marker {Guid.NewGuid()}", EntityTag.Pickup, entity =>
        {
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
        var blipEntity = _ecs.CreateEntity(constructionInfo?.Id ?? $"marker {Guid.NewGuid()}", EntityTag.Blip, entity =>
        {
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
        if (playerEntity.Tag != EntityTag.Player)
            throw new ArgumentException("Entity must be a player entity");

        var blip = new Blip(position, blipIcon, 250);

        var blipElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new BlipElementComponent(blip)));
        playerEntity.Transform.Position = position;
        AssociateWithPlayerEntity(blipElementComponent, playerEntity);
        return blipElementComponent;
    }

    public Entity CreateRadarArea(Vector2 position, Vector2 size, Color color, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        var blipEntity = _ecs.CreateEntity(constructionInfo?.Id ?? $"radarArea {Guid.NewGuid()}", EntityTag.RadarArea, entity =>
        {
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
        if (playerEntity.Tag != EntityTag.Player)
            throw new ArgumentException("Entity must be a player entity");

        var radarArea = new RadarArea(position, size, color);

        var radarAreaElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new RadarAreaElementComponent(radarArea)));
        AssociateWithPlayerEntity(radarAreaElementComponent, playerEntity);
        return radarAreaElementComponent;
    }

    public PlayerPrivateElementComponent<CollisionSphereElementComponent> CreateCollisionSphereFor(Entity playerEntity, Vector3 position, float radius)
    {
        if (playerEntity.Tag != EntityTag.Player)
            throw new ArgumentException("Entity must be a player entity");

        var collisionSphere = new CollisionSphere(position, radius);

        var collisionSphereElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new CollisionSphereElementComponent(collisionSphere)));
        AssociateWithPlayerEntity(collisionSphereElementComponent, playerEntity);
        return collisionSphereElementComponent;
    }

    public PlayerPrivateElementComponent<MarkerElementComponent> CreateMarkerFor(Entity playerEntity, Vector3 position, MarkerType markerType, Color? color = null)
    {
        if (playerEntity.Tag != EntityTag.Player)
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
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"object {Guid.NewGuid()}", EntityTag.WorldObject, entity =>
        {
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
        if (playerEntity.Tag != EntityTag.Player)
            throw new ArgumentException("Entity must be a player entity");

        var worldObject = new WorldObject(model, position)
        {
            Rotation = rotation
        };

        var worldObjectElementComponent = playerEntity.AddComponent(PlayerPrivateElementComponent.Create(new WorldObjectComponent(worldObject)));
        AssociateWithPlayerEntity(worldObjectElementComponent, playerEntity);
        return worldObjectElementComponent;
    }

    #region Collision shapes
    public Entity CreateCollisionCircle(Vector2 position, float radius, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision circle {Guid.NewGuid()}", EntityTag.CollisionShape, entity =>
        {
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
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision cuboid {Guid.NewGuid()}", EntityTag.CollisionShape, entity =>
        {
            var collisioncuboid = new CollisionCuboid(position, dimensions);

            entity.Transform.Position = position;
            if (constructionInfo != null)
            {
                entity.Transform.Interior = constructionInfo.Interior;
                entity.Transform.Dimension = constructionInfo.Dimension;
            }

            var collisionCuboidElementComponent = entity.AddComponent(new CollisionCuboidElementComponent(collisioncuboid));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null)
    {
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision polygon {Guid.NewGuid()}", EntityTag.CollisionShape, entity =>
        {
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
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision rectangle {Guid.NewGuid()}", EntityTag.CollisionShape, entity =>
        {
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
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision sphere {Guid.NewGuid()}", EntityTag.CollisionShape, entity =>
        {
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
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"collision tube {Guid.NewGuid()}", EntityTag.CollisionShape, entity =>
        {
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
        return _ecs.CreateEntity(constructionInfo?.Id ?? $"ped {Guid.NewGuid()}", EntityTag.Ped, entity =>
        {
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
