using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using System.Drawing;

namespace RealmCore.Tests.Tests.Vehicles;

public class LoadVehicles
{
    [Fact]
    public async Task Test1()
    {
        var realmTestingServer = new RealmTestingServer(configureServices: x =>
        {
            x.AddScoped<IElementFactory, TestElementFactory>();
        });

        var factory = realmTestingServer.GetRequiredService<IElementFactory>();
        var vehicleService = realmTestingServer.GetRequiredService<IVehiclesService>();
        var saveService = realmTestingServer.GetRequiredService<ISaveService>();
        var loadService = realmTestingServer.GetRequiredService<ILoadService>();

        var vehicle = await vehicleService.CreatePersistantVehicle(404, Vector3.Zero, Vector3.Zero);
        vehicle.Should().BeOfType<TestRealmVehicle>();

        await saveService.Save(vehicle);
        vehicle.Destroy();

        var loadedVehicle = await loadService.LoadVehicleById(vehicle.PersistentId);
        loadedVehicle.Should().BeOfType<TestRealmVehicle>();
    }
}

internal class TestRealmVehicle : RealmVehicle
{
    public TestRealmVehicle(IServiceProvider serviceProvider, ushort model, Vector3 position) : base(serviceProvider, model, position)
    {
    }
}

internal class TestElementFactory : IElementFactory
{
    private readonly IElementFactory _innerFactory;
    private readonly IServiceProvider _serviceProvider;

    public event Action<Element>? ElementCreated;

    public TestElementFactory([FromKeyedServices("ElementFactory")] IElementFactory innerFactory, IServiceProvider serviceProvider)
    {
        _innerFactory = innerFactory;
        _serviceProvider = serviceProvider;
        _innerFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        ElementCreated?.Invoke(element);
    }

    public void AssociateWithServer(Element element)
    {
        _innerFactory.AssociateWithServer(element);
    }

    public RealmBlip CreateBlip(Vector3 position, BlipIcon blipIcon, byte? interior = null, ushort? dimension = null, Action<RealmBlip>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCircle>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCuboid>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Action<RealmCollisionPolygon>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionRectangle>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionSphere>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Action<RealmCollisionTube>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public FocusableRealmWorldObject CreateFocusableObject(ObjectModel model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Action<RealmWorldObject>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmMarker CreateMarker(Vector3 position, MarkerType markerType, float size, Color color, byte? interior = null, ushort? dimension = null, Action<RealmMarker>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmWorldObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Action<RealmWorldObject>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmPed CreatePed(PedModel pedModel, Vector3 position, byte? interior = null, ushort? dimension = null, Action<RealmPed>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmPickup CreatePickup(Vector3 position, ushort model, byte? interior = null, ushort? dimension = null, Action<RealmPickup>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Action<RealmRadarArea>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Action<RealmVehicle>? elementBuilder = null)
    {
        var vehicle = new TestRealmVehicle(_serviceProvider, model, position)
        {
            Rotation = rotation,
            Interior = interior ?? 0,
            Dimension = dimension ?? 0,
        };

        elementBuilder?.Invoke(vehicle);
        AssociateWithServer(vehicle);
        return vehicle;
    }

    public void RelayCreated(Element element)
    {

    }
}