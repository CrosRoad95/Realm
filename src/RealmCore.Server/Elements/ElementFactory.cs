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

    public RealmMarker CreateMarker(Vector3 position, MarkerType markerType, float size, Color color, byte? interior = null, ushort? dimension = null, Func<RealmMarker, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var marker = new RealmMarker(_serviceProvider, position, markerType, size)
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

    public RealmRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Func<RealmRadarArea, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var radarArea = new RealmRadarArea(_serviceProvider, position, size, color)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        ExecuteElementBuilder(elementBuilder, radarArea);
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
    public RealmCollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Func<RealmCollisionCircle, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var collisionSphere = new RealmCollisionCircle(_serviceProvider, new Vector2(0, 0), radius)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(collisionSphere);
        AssociateWithServer(collisionSphere);
        return collisionSphere;
    }

    public RealmCollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Func<RealmCollisionCuboid, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var collisionCuboid = new RealmCollisionCuboid(_serviceProvider, position, dimensions)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(collisionCuboid);
        AssociateWithServer(collisionCuboid);
        return collisionCuboid;
    }

    public RealmCollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Func<RealmCollisionPolygon, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var collisionPolygon = new RealmCollisionPolygon(_serviceProvider, position, vertices)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(collisionPolygon);
        AssociateWithServer(collisionPolygon);
        return collisionPolygon;
    }

    public RealmCollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Func<RealmCollisionRectangle, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var collisionRectangle = new RealmCollisionRectangle(_serviceProvider, position, dimensions)
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

    public RealmCollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Func<RealmCollisionTube, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var collisionTube = new RealmCollisionTube(_serviceProvider, position, radius, height)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(collisionTube);
        AssociateWithServer(collisionTube);
        return collisionTube;
    }

    public RealmPed CreatePed(PedModel pedModel, Vector3 position, byte? interior = null, ushort? dimension = null, Func<RealmPed, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var ped = new RealmPed(_serviceProvider, pedModel, position)
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
