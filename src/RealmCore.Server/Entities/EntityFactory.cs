using Vehicle = SlipeServer.Server.Elements.Vehicle;

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
        var elementComponent = entity.GetRequiredComponent<ElementComponent>();

        if (!elementComponent.BaseLoaded)
            throw new Exception("Failed to load element entity, base.Load was not called.");

        var element = elementComponent.Element;
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
            entity.AddComponent(new Transform(position, rotation, interior, dimension));

            entity.AddComponent<VehicleTagComponent>();
            var vehicle = new Vehicle(model, position);

            var vehicleElementComponent = entity.AddComponent(new VehicleElementComponent(vehicle, _entityEngine));

            entityBuilder?.Invoke(entity);
            AssociateWithServer(entity);
        });

        return vehicleEntity;
    }

    public Entity CreateMarker(MarkerType markerType, Vector3 position, Color color, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(position, interior, dimension));
            entity.AddComponent<MarkerTagComponent>();

            var markerElementComponent = entity.AddComponent(new MarkerElementComponent(new Marker(new Vector3(0, 0, 1000), markerType)
            {
                Color = color
            }, _elementCollection, _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreatePickup(ushort model, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        var pickupEntity = _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(position, interior, dimension));
            entity.AddComponent<PickupTagComponent>();
            entity.AddComponent(new PickupElementComponent(new Pickup(Vector3.Zero, model), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return pickupEntity;
    }

    public Entity CreateBlip(BlipIcon blipIcon, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        var blipEntity = _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(position, interior, dimension));
            entity.AddComponent<BlipTagComponent>();
            entity.AddComponent(new BlipElementComponent(new Blip(Vector3.Zero, blipIcon, 250)));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return blipEntity;
    }

    public Entity CreateRadarArea(Vector2 position, Vector2 size, Color color, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        var blipEntity = _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(new Vector3(position, 0), interior, dimension));
            entity.AddComponent<RadarAreaTagComponent>();
            entity.AddComponent(new RadarAreaElementComponent(new RadarArea(position, size, color)));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });

        return blipEntity;
    }

    public Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(position, rotation, interior, dimension));
            entity.AddComponent<WorldObjectTagComponent>();
            entity.AddComponent(new WorldObjectComponent(new WorldObject(model, position)));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    #region Collision shapes
    public Entity CreateCollisionCircle(Vector2 position, float radius, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(new Vector3(position, 0), interior, dimension));
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionCircleElementComponent(new CollisionCircle(new Vector2(0, 0), radius), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(position, interior, dimension));
            entity.AddComponent<CollisionShapeTagComponent>();

            var collisionCuboidElementComponent = entity.AddComponent(new CollisionCuboidElementComponent(new CollisionCuboid(position, dimensions), _entityEngine));

            AssociateWithServer(entity);

            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(position, interior, dimension));
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionPolygonElementComponent(new CollisionPolygon(position, vertices), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(new Vector3(position, 0), interior, dimension));
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionRectangleElementComponent(new CollisionRectangle(position, dimensions), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(position, interior, dimension));
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionSphereElementComponent(new CollisionSphere(new Vector3(0, 0, 1000), radius), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreateCollisionTube(Vector3 position, float radius, float height, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(position, interior, dimension));
            entity.AddComponent<CollisionShapeTagComponent>();
            entity.AddComponent(new CollisionTubeElementComponent(new CollisionTube(position, radius, height), _entityEngine));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    public Entity CreatePed(PedModel pedModel, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null)
    {
        return _entityEngine.CreateEntity(entity =>
        {
            entity.AddComponent(new Transform(position, interior, dimension));
            entity.AddComponent<PedTagComponent>();
            entity.AddComponent(new PedElementComponent(new Ped(pedModel, position)));

            AssociateWithServer(entity);
            entityBuilder?.Invoke(entity);
        });
    }

    #endregion
}
