namespace RealmCore.Server.Entities;

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
        ElementCreated?.Invoke(element);
    }

    public RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<RealmVehicle>? elementBuilder = null)
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

        AssociateWithServer(marker);
        elementBuilder?.Invoke(marker);
        return marker;
    }

    public RealmPickup CreatePickup(Vector3 position, ushort model, byte interior = 0, ushort dimension = 0, Action<RealmPickup>? elementBuilder = null)
    {
        var pickup = new RealmPickup(_serviceProvider, position, model)
        {
            Interior = interior,
            Dimension = dimension,
        };

        AssociateWithServer(pickup);
        elementBuilder?.Invoke(pickup);
        return pickup;
    }

    public Blip CreateBlip(Vector3 position, BlipIcon blipIcon, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var blip = new Blip(position, blipIcon)
        {
            Interior = interior,
            Dimension = dimension,
        };

        AssociateWithServer(blip);
        elementBuilder?.Invoke(blip);
        return blip;
    }

    public RadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var radarArea = new RadarArea(position, size, color)
        {
            Interior = interior,
            Dimension = dimension,
        };

        AssociateWithServer(radarArea);
        elementBuilder?.Invoke(radarArea);
        return radarArea;
    }

    public RealmObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<RealmObject>? elementBuilder = null)
    {
        var worldObject = new RealmObject(_serviceProvider, model, position)
        {
            Rotation = rotation,
            Interior = interior,
            Dimension = dimension,
        };

        AssociateWithServer(worldObject);
        elementBuilder?.Invoke(worldObject);
        return worldObject;
    }

    #region Collision shapes
    public CollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var collisionSphere = new CollisionCircle(new Vector2(0, 0), radius);
        AssociateWithServer(collisionSphere);
        elementBuilder?.Invoke(collisionSphere);
        return collisionSphere;
    }

    public CollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var collisionCuboid = new CollisionCuboid(position, dimensions);
        AssociateWithServer(collisionCuboid);
        elementBuilder?.Invoke(collisionCuboid);
        return collisionCuboid;
    }

    public CollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var collisionPolygon = new CollisionPolygon(position, vertices);
        AssociateWithServer(collisionPolygon);
        elementBuilder?.Invoke(collisionPolygon);
        return collisionPolygon;
    }

    public CollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var collisionRectangle = new CollisionRectangle(position, dimensions)
        {
            Interior = interior,
            Dimension = dimension,
        };

        AssociateWithServer(collisionRectangle);
        elementBuilder?.Invoke(collisionRectangle);
        return collisionRectangle;
    }

    public RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, Action<RealmCollisionSphere>? elementBuilder = null)
    {
        var collisionSphere = new RealmCollisionSphere(_serviceProvider, new Vector3(0, 0, 1000), radius);
        AssociateWithServer(collisionSphere);
        elementBuilder?.Invoke(collisionSphere);
        return collisionSphere;
    }

    public CollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var collisionTube = new CollisionTube(position, radius, height)
        {
            Interior = interior,
            Dimension = dimension,
        };

        AssociateWithServer(collisionTube);
        elementBuilder?.Invoke(collisionTube);
        return collisionTube;
    }

    public Ped CreatePed(PedModel pedModel, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null)
    {
        var ped = new Ped(pedModel, position)
        {
            Interior = interior,
            Dimension = dimension,
        };

        AssociateWithServer(ped);
        elementBuilder?.Invoke(ped);
        return ped;
    }
    #endregion
}
