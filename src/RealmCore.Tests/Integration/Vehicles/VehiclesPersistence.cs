﻿namespace RealmCore.Tests.Integration.Vehicles;

public class VehiclesPersistence : RealmIntegrationTestingBase
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
        var vehicleService = server.GetRequiredService<IVehiclesService>();
        var saveService = server.GetRequiredService<ISaveService>();
        var loadService = server.GetRequiredService<ILoadService>();

        var vehicle = await vehicleService.CreatePersistantVehicle(404, Vector3.Zero, Vector3.Zero);
        vehicle.Should().BeOfType<TestRealmVehicle>();

        await saveService.Save(vehicle);
        vehicle.Destroy();

        var loadedVehicle = await loadService.LoadVehicleById(vehicle.PersistentId);
        loadedVehicle.Should().BeOfType<TestRealmVehicle>();
    }


    [Fact]
    public async Task SpawningTwoPersistentVehiclesShouldNotBeAllowed()
    {
        var server = await CreateServerAsync();
        var vehiclesService = server.GetRequiredService<IVehiclesService>();
        var loadService = server.GetRequiredService<ILoadService>();
        var saveService = server.GetRequiredService<ISaveService>();
        var activeVehicles = server.GetRequiredService<IVehiclesInUse>();
        var vehicle = await vehiclesService.CreatePersistantVehicle(404, Vector3.Zero, Vector3.Zero);
        var id = vehicle.PersistentId;
        activeVehicles.ActiveVehiclesIds.Should().BeEquivalentTo([id]);
        activeVehicles.IsActive(id).Should().BeTrue();
        activeVehicles.TryGetVehicleById(id, out var foundVehicle).Should().BeTrue();
        foundVehicle.Should().Be(vehicle);
        await saveService.Save(vehicle);
        vehicle.Destroy();

        var spawn = async () => await loadService.LoadVehicleById(id);
        spawn.Should().NotThrow();
        spawn.Should().Throw<PersistantVehicleAlreadySpawnedException>().WithMessage("Failed to create already existing vehicle.");
    }

    [Fact]
    public async Task SpawnedVehicleShouldBeExactlyTheSameAsSavedOne()
    {
        var server = await CreateServerAsync();
        server.GetRequiredService<VehicleUpgradesCollection>().Add(250, new VehicleUpgradesCollectionItem(EmptyVehicleHandlingModifier.Instance));
        var player = server.CreatePlayer();
        await server.SignInPlayer(player);
        var vehiclesService = server.GetRequiredService<IVehiclesService>();
        var loadService = server.GetRequiredService<ILoadService>();
        var saveService = server.GetRequiredService<ISaveService>();
        var vehicle1 = await vehiclesService.CreatePersistantVehicle(404, new Vector3(1, 2, 3), new Vector3(4, 5, 6));
        vehicle1.Access.AddAsOwner(player);
        vehicle1.MileageCounter.Mileage = 123;
        vehicle1.Upgrades.AddUpgrade(250, false);
        vehicle1.PartDamage.AddPart(200, 300);
        vehicle1.Engines.Add(50);
        var id = vehicle1.PersistentId;
        await saveService.Save(vehicle1);
        vehicle1.Destroy();

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