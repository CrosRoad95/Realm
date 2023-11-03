namespace RealmCore.Server.Elements;

internal sealed class ElementFactory : IElementFactory
{
    private readonly MtaServer _mtaServer;
    private readonly IServiceProvider _serviceProvider;

    public event Action<Element>? ElementCreated;
    public ElementFactory(MtaServer mtaServer, IServiceProvider serviceProvider)
    {
        _mtaServer = mtaServer;
        _serviceProvider = serviceProvider;
        _mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        ElementCreated?.Invoke(player);
    }

    private void ExecuteElementBuilder<TElement>(Func<TElement, IEnumerable<IComponent>>? builder, TElement element) where TElement : IComponents
    {
        if (builder == null)
            return;

        var components = builder(element);
        foreach (var component in components)
        {
            element.AddComponent(component);
        }
    }

    public void RelayCreated(Element element)
    {
        ElementCreated?.Invoke(element);
    }

    private void AssociateWithServer(Element element)
    {
        element.AssociateWith(_mtaServer);
        if (element is Pickup pickup)
        {
            pickup.CollisionShape.AssociateWith(_mtaServer);
        }
        if (element is RealmMarker marker)
        {
            marker.CollisionShape.AssociateWith(_mtaServer);
        }
        RelayCreated(element);
    }

    public RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Func<RealmVehicle, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var vehicle = new RealmVehicle(_serviceProvider, model, position)
        {
            Rotation = rotation,
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(vehicle);
        AssociateWithServer(vehicle);
        return vehicle;
    }

    public RealmMarker CreateMarker(Vector3 position, MarkerType markerType, Color color, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        var marker = new RealmMarker(_serviceProvider, position, markerType, 2)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
            Color = color
        };

        elementBuilder?.Invoke(marker);
        AssociateWithServer(marker);
        return marker;
    }

    public RealmPickup CreatePickup(Vector3 position, ushort model, byte? interior = null, ushort? dimension = null, Func<RealmPickup, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var pickup = new RealmPickup(_serviceProvider, position, model)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        ExecuteElementBuilder(elementBuilder, pickup);
        AssociateWithServer(pickup);
        return pickup;
    }

    public RealmBlip CreateBlip(Vector3 position, BlipIcon blipIcon, byte? interior = null, ushort? dimension = null, Func<RealmBlip, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var blip = new RealmBlip(_serviceProvider, position, blipIcon)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        ExecuteElementBuilder(elementBuilder, blip);
        AssociateWithServer(blip);
        return blip;
    }

    public RadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        var radarArea = new RadarArea(position, size, color)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(radarArea);
        AssociateWithServer(radarArea);
        return radarArea;
    }

    public RealmObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Func<RealmObject, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var worldObject = new RealmObject(_serviceProvider, model, position)
        {
            Rotation = rotation,
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(worldObject);
        AssociateWithServer(worldObject);
        return worldObject;
    }

    #region Collision shapes
    public CollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        var collisionSphere = new CollisionCircle(new Vector2(0, 0), radius)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(collisionSphere);
        AssociateWithServer(collisionSphere);
        return collisionSphere;
    }

    public CollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        var collisionCuboid = new CollisionCuboid(position, dimensions)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(collisionCuboid);
        AssociateWithServer(collisionCuboid);
        return collisionCuboid;
    }

    public CollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        var collisionPolygon = new CollisionPolygon(position, vertices)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(collisionPolygon);
        AssociateWithServer(collisionPolygon);
        return collisionPolygon;
    }

    public CollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        var collisionRectangle = new CollisionRectangle(position, dimensions)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(collisionRectangle);
        AssociateWithServer(collisionRectangle);
        return collisionRectangle;
    }

    public RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Func<RealmCollisionSphere, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var collisionSphere = new RealmCollisionSphere(_serviceProvider, position, radius)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(collisionSphere);
        AssociateWithServer(collisionSphere);
        return collisionSphere;
    }

    public CollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        var collisionTube = new CollisionTube(position, radius, height)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(collisionTube);
        AssociateWithServer(collisionTube);
        return collisionTube;
    }

    public Ped CreatePed(PedModel pedModel, Vector3 position, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null)
    {
        var ped = new Ped(pedModel, position)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(ped);
        AssociateWithServer(ped);
        return ped;
    }
    #endregion
}
