namespace RealmCore.Server.Modules.Elements;

public class ElementFactory : IElementFactory
{
    private readonly MtaServer _server;
    private readonly IServiceProvider _serviceProvider;

    public event Action<Element>? ElementCreated;
    public ElementFactory(MtaServer server, IPlayersEventManager playersEventManager, IServiceProvider serviceProvider)
    {
        _server = server;
        _serviceProvider = serviceProvider;
        playersEventManager.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        ElementCreated?.Invoke(player);
    }

    public void RelayCreated(Element element)
    {
        ElementCreated?.Invoke(element);
    }

    public void AssociateWithServer(Element element)
    {
        element.AssociateWith(_server);
        if (element is Pickup pickup)
        {
            pickup.CollisionShape.AssociateWith(_server);
            RelayCreated(pickup.CollisionShape);
        }
        if (element is RealmMarker marker)
        {
            marker.CollisionShape.AssociateWith(_server);
            RelayCreated(marker.CollisionShape);
        }
        RelayCreated(element);
    }

    public RealmVehicle CreateVehicle(Location location, VehicleModel model, Action<RealmVehicle>? elementBuilder = null)
    {
        var vehicle = new RealmVehicle(_serviceProvider, (ushort)model, location.Position)
        {
            Rotation = location.Rotation,
            Interior = location.GetInteriorOrDefault(),
            Dimension = location.GetDimensionOrDefault(),
        };

        try
        {
            elementBuilder?.Invoke(vehicle);
        }
        catch (Exception)
        {
            vehicle.Destroy();
            throw;
        }

        AssociateWithServer(vehicle);
        return vehicle;
    }
    
    public async Task<RealmVehicle> CreateVehicle(Location location, VehicleModel model, Func<RealmVehicle, Task> elementBuilder)
    {
        var vehicle = new RealmVehicle(_serviceProvider, (ushort)model, location.Position)
        {
            Rotation = location.Rotation,
            Interior = location.GetInteriorOrDefault(),
            Dimension = location.GetDimensionOrDefault(),
        };

        try
        {
            await elementBuilder.Invoke(vehicle);
        }
        catch (Exception)
        {
            vehicle.Destroy();
            throw;
        }

        AssociateWithServer(vehicle);
        return vehicle;
    }
    
    public RealmMarker CreateMarker(Location location, MarkerType markerType, float size, Color color, Action<RealmMarker>? elementBuilder = null)
    {
        var marker = new RealmMarker(_serviceProvider, location.Position, markerType, size)
        {
            Interior = location.GetInteriorOrDefault(),
            Dimension = location.GetDimensionOrDefault(),
            Color = color
        };
        marker.CollisionShape.Interior = location.GetInteriorOrDefault();
        marker.CollisionShape.Dimension = location.GetDimensionOrDefault();

        elementBuilder?.Invoke(marker);
        AssociateWithServer(marker);
        return marker;
    }

    public RealmPickup CreatePickup(Location location, ushort model, Action<RealmPickup>? elementBuilder = null)
    {
        var pickup = new RealmPickup(_serviceProvider, location.Position, model)
        {
            Interior = location.GetInteriorOrDefault(),
            Dimension = location.GetDimensionOrDefault(),
        };
        pickup.CollisionShape.Interior = location.GetInteriorOrDefault();
        pickup.CollisionShape.Dimension = location.GetDimensionOrDefault();

        elementBuilder?.Invoke(pickup);
        AssociateWithServer(pickup);
        return pickup;
    }

    public RealmBlip CreateBlip(Location location, BlipIcon blipIcon, Action<RealmBlip>? elementBuilder = null)
    {
        var blip = new RealmBlip(location.Position, blipIcon)
        {
            Interior = location.GetInteriorOrDefault(),
            Dimension = location.GetDimensionOrDefault(),
        };

        elementBuilder?.Invoke(blip);
        AssociateWithServer(blip);
        return blip;
    }

    public RealmRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Action<RealmRadarArea>? elementBuilder = null)
    {
        var radarArea = new RealmRadarArea(position, size, color)
        {
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(radarArea);
        AssociateWithServer(radarArea);
        return radarArea;
    }

    public RealmWorldObject CreateObject(Location location, ObjectModel model, Action<RealmWorldObject>? elementBuilder = null)
    {
        var worldObject = new RealmWorldObject(model, location.Position)
        {
            Rotation = location.Rotation,
            Interior = location.GetInteriorOrDefault(),
            Dimension = location.GetDimensionOrDefault(),
        };

        elementBuilder?.Invoke(worldObject);
        AssociateWithServer(worldObject);
        return worldObject;
    }

    public FocusableRealmWorldObject CreateFocusableObject(Location location, ObjectModel model, Action<RealmWorldObject>? elementBuilder = null)
    {
        var worldObject = new FocusableRealmWorldObject(model, location.Position)
        {
            Rotation = location.Rotation,
            Interior = location.GetInteriorOrDefault(),
            Dimension = location.GetDimensionOrDefault(),
        };

        elementBuilder?.Invoke(worldObject);
        AssociateWithServer(worldObject);
        return worldObject;
    }

    #region Collision shapes
    public RealmCollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCircle>? elementBuilder = null)
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

    public RealmCollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCuboid>? elementBuilder = null)
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

    public RealmCollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Action<RealmCollisionPolygon>? elementBuilder = null)
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

    public RealmCollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionRectangle>? elementBuilder = null)
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

    public RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionSphere>? elementBuilder = null)
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

    public RealmCollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Action<RealmCollisionTube>? elementBuilder = null)
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

    public RealmPed CreatePed(Location location, PedModel pedModel, Action<RealmPed>? elementBuilder = null)
    {
        var ped = new RealmPed(pedModel, location.Position)
        {
            Rotation = location.Rotation,
            Interior = location.GetInteriorOrDefault(),
            Dimension = location.GetDimensionOrDefault(),
        };

        elementBuilder?.Invoke(ped);
        AssociateWithServer(ped);
        return ped;
    }
    #endregion
}
