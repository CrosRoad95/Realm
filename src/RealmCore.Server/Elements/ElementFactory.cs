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

    public RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Func<RealmVehicle, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var vehicle = new RealmVehicle(_serviceProvider, model, position)
        {
            Rotation = rotation,
            Interior = interior,
            Dimension = dimension,
        };

        elementBuilder?.Invoke(vehicle);
        AssociateWithServer(vehicle);
        return vehicle;
    }

    public RealmMarker CreateMarker(Vector3 position, MarkerType markerType, Color color, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var marker = new RealmMarker(_serviceProvider, position, markerType, 2)
        {
            Interior = interior,
            Dimension = dimension,
            Color = color
        };

        elementBuilder?.Invoke(marker);
        AssociateWithServer(marker);
        return marker;
    }

    public RealmPickup CreatePickup(Vector3 position, ushort model, byte interior = 0, ushort dimension = 0, Func<RealmPickup, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var pickup = new RealmPickup(_serviceProvider, position, model)
        {
            Interior = interior,
            Dimension = dimension,
        };

        ExecuteElementBuilder(elementBuilder, pickup);
        AssociateWithServer(pickup);
        return pickup;
    }

    public RealmBlip CreateBlip(Vector3 position, BlipIcon blipIcon, byte interior = 0, ushort dimension = 0, Func<RealmBlip, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var blip = new RealmBlip(_serviceProvider, position, blipIcon)
        {
            Interior = interior,
            Dimension = dimension,
        };

        ExecuteElementBuilder(elementBuilder, blip);
        AssociateWithServer(blip);
        return blip;
    }

    public RadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var radarArea = new RadarArea(position, size, color)
        {
            Interior = interior,
            Dimension = dimension,
        };

        elementBuilder?.Invoke(radarArea);
        AssociateWithServer(radarArea);
        return radarArea;
    }

    public RealmObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Func<RealmObject, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var worldObject = new RealmObject(_serviceProvider, model, position)
        {
            Rotation = rotation,
            Interior = interior,
            Dimension = dimension,
        };

        elementBuilder?.Invoke(worldObject);
        AssociateWithServer(worldObject);
        return worldObject;
    }

    #region Collision shapes
    public CollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var collisionSphere = new CollisionCircle(new Vector2(0, 0), radius);
        elementBuilder?.Invoke(collisionSphere);
        AssociateWithServer(collisionSphere);
        return collisionSphere;
    }

    public CollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var collisionCuboid = new CollisionCuboid(position, dimensions);
        elementBuilder?.Invoke(collisionCuboid);
        AssociateWithServer(collisionCuboid);
        return collisionCuboid;
    }

    public CollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var collisionPolygon = new CollisionPolygon(position, vertices);
        elementBuilder?.Invoke(collisionPolygon);
        AssociateWithServer(collisionPolygon);
        return collisionPolygon;
    }

    public CollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var collisionRectangle = new CollisionRectangle(position, dimensions)
        {
            Interior = interior,
            Dimension = dimension,
        };

        elementBuilder?.Invoke(collisionRectangle);
        AssociateWithServer(collisionRectangle);
        return collisionRectangle;
    }

    public RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, Func<RealmCollisionSphere, IEnumerable<IComponent>>? elementBuilder = null)
    {
        var collisionSphere = new RealmCollisionSphere(_serviceProvider, new Vector3(0, 0, 1000), radius);
        elementBuilder?.Invoke(collisionSphere);
        AssociateWithServer(collisionSphere);
        return collisionSphere;
    }

    public CollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var collisionTube = new CollisionTube(position, radius, height)
        {
            Interior = interior,
            Dimension = dimension,
        };

        elementBuilder?.Invoke(collisionTube);
        AssociateWithServer(collisionTube);
        return collisionTube;
    }

    public Ped CreatePed(PedModel pedModel, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var ped = new Ped(pedModel, position)
        {
            Interior = interior,
            Dimension = dimension,
        };

        elementBuilder?.Invoke(ped);
        AssociateWithServer(ped);
        return ped;
    }
    #endregion
}
