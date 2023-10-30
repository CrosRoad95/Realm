namespace RealmCore.Server.Entities;

internal sealed class EntityFactory : IEntityFactory
{
    private readonly IEntityEngine _entityEngine;
    private readonly IElementCollection _elementCollection;
    private readonly MtaServer _mtaServer;

    public EntityFactory(IEntityEngine entityEngine, IElementCollection elementCollection, MtaServer mtaServer)
    {
        _entityEngine = entityEngine;
        _elementCollection = elementCollection;
        _mtaServer = mtaServer;
    }

    public IScopedEntityFactory CreateScopedEntityFactory(Entity entity)
    {
        return new ScopedEntityFactory(entity, _elementCollection, _entityEngine);
    }

    private void AssociateWithServer(Entity entity)
    {
        //if (!_realmServer.IsReady)
        //    throw new InvalidOperationException("Could not create entities in logics constructors.");
        var elementComponent = entity.GetRequiredComponent<IElementComponent>();

        var element = (Element)elementComponent;
        element.AssociateWith(_mtaServer);
        if (element is Pickup pickup)
        {
            pickup.CollisionShape.AssociateWith(_mtaServer);
        }
        if (elementComponent is MarkerElementComponent markerElementComponent)
        {
            markerElementComponent.CollisionShape.AssociateWith(_mtaServer);
        }
    }

    public Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        var vehicleEntity = _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<VehicleTagComponent>();

            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(model, position));

            entityBuilder?.Invoke(entity);
            AssociateWithServer(entity);
        });

        return vehicleEntity;
    }

    public Entity CreateMarker(MarkerType markerType, Vector3 position, Color color, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<MarkerTagComponent>();

            var markerElementComponent = entity.AddComponent(new MarkerElementComponent(new Vector3(0, 0, 1000), 2, markerType)
            {
                Color = color
            });

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreatePickup(ushort model, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        var pickupEntity = _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<PickupTagComponent>();
            entity.AddComponent(new PickupElementComponent(Vector3.Zero, model));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return pickupEntity;
    }

    public Entity CreateBlip(BlipIcon blipIcon, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        var blipEntity = _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<BlipTagComponent>();
            entity.AddComponent(new BlipElementComponent(Vector3.Zero, blipIcon, 250));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return blipEntity;
    }

    public Entity CreateRadarArea(Vector2 position, Vector2 size, Color color, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        var blipEntity = _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<RadarAreaTagComponent>();
            entity.AddComponent(new RadarAreaElementComponent(position, size, color));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return blipEntity;
    }

    public Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<WorldObjectTagComponent>();
            entity.AddComponent(new WorldObjectComponent(model, position));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    #region Collision shapes
    public Entity CreateCollisionCircle(Vector2 position, float radius, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionCircleElementComponent(new Vector2(0, 0), radius));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();

            var collisionCuboidElementComponent = entity.AddComponent(new CollisionCuboidElementComponent(position, dimensions));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionPolygonElementComponent(position, vertices));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionRectangleElementComponent(position, dimensions));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionSphereElementComponent(new Vector3(0, 0, 1000), radius));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionTube(Vector3 position, float radius, float height, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionTubeElementComponent(position, radius, height));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreatePed(PedModel pedModel, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent<PedTagComponent>();
            entity.AddComponent(new PedElementComponent(pedModel, position));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    #endregion
}
