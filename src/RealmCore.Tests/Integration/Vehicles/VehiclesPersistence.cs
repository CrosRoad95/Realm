using RealmCore.Server.Modules.Elements.Focusable;
using RealmCore.Server.Modules.Pickups;

namespace RealmCore.Tests.Integration.Vehicles;

[Collection("IntegrationTests")]
public class VehiclesPersistence : RealmRemoteDatabaseIntegrationTestingBase
{
    protected override string DatabaseName => "VehiclesPersistence";

    [Fact]
    public async Task Test1()
    {
        var server = await CreateServerAsync(configureServices: x =>
        {
            x.AddScoped<IElementFactory, TestElementFactory>();
        });

        var factory = server.GetRequiredService<IElementFactory>();
        var vehiclesService = server.GetRequiredService<IVehiclesService>();
        var loadService = server.GetRequiredService<IVehicleLoader>();

        var vehicle = await vehiclesService.CreatePersistantVehicle(Location.Zero, (VehicleModel)404);
        vehicle.Should().BeOfType<TestRealmVehicle>();

        await vehicle.GetRequiredService<ISaveService>().Save();
        await vehiclesService.Destroy(vehicle);

        var loadedVehicle = await loadService.LoadVehicleById(vehicle.PersistentId);
        loadedVehicle.Should().BeOfType<TestRealmVehicle>();
    }

    [Fact]
    public async Task SpawningTwoPersistentVehiclesShouldNotBeAllowed()
    {
        var server = await CreateServerAsync();
        var vehiclesService = server.GetRequiredService<IVehiclesService>();
        var loadService = server.GetRequiredService<IVehicleLoader>();
        var activeVehicles = server.GetRequiredService<IVehiclesInUse>();
        var vehicle = await vehiclesService.CreatePersistantVehicle(Location.Zero, (VehicleModel)404);
        var id = vehicle.PersistentId;
        activeVehicles.ActiveVehiclesIds.Should().BeEquivalentTo([id]);
        activeVehicles.IsActive(id).Should().BeTrue();
        activeVehicles.TryGetVehicleById(id, out var foundVehicle).Should().BeTrue();
        foundVehicle.Should().Be(vehicle);
        await vehiclesService.Destroy(vehicle);

        var spawn = async () => await loadService.LoadVehicleById(id);
        await spawn.Should().NotThrowAsync();
        await spawn.Should().ThrowAsync<PersistantVehicleAlreadySpawnedException>().WithMessage("Failed to create already existing vehicle.");
    }

    [Fact]
    public async Task SpawnedVehicleShouldBeExactlyTheSameAsSavedOne()
    {
        var server = await CreateServerAsync();
        server.GetRequiredService<VehicleUpgradesCollection>().Add(250, new VehicleUpgradesCollectionItem(EmptyVehicleHandlingModifier.Instance));
        var player = server.CreatePlayer();
        await server.SignInPlayer(player);
        var vehiclesService = server.GetRequiredService<IVehiclesService>();
        var loadService = server.GetRequiredService<IVehicleLoader>();
        var vehicle1 = await vehiclesService.CreatePersistantVehicle(new Location(new Vector3(1, 2, 3), new Vector3(4, 5, 6)), (VehicleModel)404);
        vehicle1.Access.AddAsOwner(player);
        vehicle1.MileageCounter.Mileage = 123;
        vehicle1.Upgrades.AddUpgrade(250, false);
        vehicle1.PartDamage.AddPart(200, 300);
        vehicle1.Engines.Add(50);
        var id = vehicle1.PersistentId;
        await vehiclesService.Destroy(vehicle1);

        var vehicle2 = await loadService.LoadVehicleById(id);

        vehicle1.Access.IsOwner(player).Should().BeTrue();
        vehicle2.MileageCounter.Mileage.Should().Be(123);
        vehicle2.Upgrades.Should().BeEquivalentTo([250]);
        vehicle2.PartDamage.Parts.Should().BeEquivalentTo([200]);
        vehicle2.PartDamage.GetState(200).Should().Be(300);
        vehicle2.Engines.EnginesIds.Should().BeEquivalentTo([50]);

        // TODO: use extension method
        vehicle1.Should().BeEquivalentTo(vehicle2, x =>
        {
            x.Including(y => y.Position);
            x.Including(y => y.PaintJob);
            return x;
        });
    }
}

internal class TestRealmVehicle : RealmVehicle
{
    public TestRealmVehicle(IServiceProvider serviceProvider, VehicleModel model, Vector3 position) : base(serviceProvider, (ushort)model, position)
    {
    }
}

internal class TestElementFactory : IElementFactory
{
    private readonly IElementFactory _innerFactory;
    private readonly IServiceProvider _serviceProvider;

    public event Action<IElementFactory, Element>? ElementCreated;

    public TestElementFactory([FromKeyedServices("ElementFactory")] IElementFactory innerFactory, IServiceProvider serviceProvider)
    {
        _innerFactory = innerFactory;
        _serviceProvider = serviceProvider;
        _innerFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        ElementCreated?.Invoke(this, element);
    }

    public void AssociateWithServer(Element element)
    {
        _innerFactory.AssociateWithServer(element);
    }

    public RealmBlip CreateBlip(Location location, BlipIcon blipIcon, Action<RealmBlip>? elementBuilder = null)
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

    public FocusableRealmWorldObject CreateFocusableObject(Location location, ObjectModel model, Action<RealmWorldObject>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmMarker CreateMarker(Location location, MarkerType markerType, float size, Color color, Action<RealmMarker>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmWorldObject CreateObject(Location location, ObjectModel model, Action<RealmWorldObject>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmPed CreatePed(Location location, PedModel pedModel, Action<RealmPed>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmPickup CreatePickup(Location location, ushort model, Action<RealmPickup>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Action<RealmRadarArea>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmVehicle CreateVehicle(Location location, VehicleModel model, Action<RealmVehicle>? elementBuilder = null)
    {
        var vehicle = new TestRealmVehicle(_serviceProvider, model, location.Position)
        {
            Rotation = location.Rotation,
            Interior = location.Interior ?? 0,
            Dimension = location.Dimension ?? 0,
        };

        elementBuilder?.Invoke(vehicle);
        AssociateWithServer(vehicle);
        return vehicle;
    }

    public void RelayCreated(Element element)
    {

    }

    public async Task<RealmVehicle> CreateVehicle(Location location, VehicleModel model, Func<RealmVehicle, Task> elementBuilder)
    {
        var vehicle = new TestRealmVehicle(_serviceProvider, model, location.Position)
        {
            Rotation = location.Rotation,
            Interior = location.Interior ?? 0,
            Dimension = location.Dimension ?? 0,
        };

        if(elementBuilder != null)
            await elementBuilder(vehicle);
        AssociateWithServer(vehicle);
        return vehicle;
    }
}