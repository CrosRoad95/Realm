using RealmCore.Server.Components.Vehicles.Access;

namespace RealmCore.Tests.Tests.Components;

public class VehicleAccessComponentsTests
{
    [Fact]
    public void PlayerShouldBeAbleToEnterVehicleIfNoAccessGetsConfigured()
    {
        var realmTestingServer = new RealmTestingServer(null);
        var entityHelper = new EntityHelper(realmTestingServer);
        var entityFactory = realmTestingServer.GetRequiredService<IEntityFactory>();

        var player = entityHelper.CreatePlayerEntity();
        var vehicle = entityFactory.CreateVehicle(404, Vector3.Zero, Vector3.Zero);
        var vehicleAccessService = realmTestingServer.GetRequiredService<IVehicleAccessService>();
        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle);
        canEnter.Should().BeTrue();
    }

    [Fact]
    public void VehicleExclusiveAccessComponentShouldWork()
    {
        var realmTestingServer = new RealmTestingServer(null);
        var entityHelper = new EntityHelper(realmTestingServer);
        var entityFactory = realmTestingServer.GetRequiredService<IEntityFactory>();

        var player1 = entityHelper.CreatePlayerEntity();
        var player2 = entityHelper.CreatePlayerEntity();
        var vehicle = entityFactory.CreateVehicle(404, Vector3.Zero, Vector3.Zero);
        var accessComponent = vehicle.AddComponent(new VehicleExclusiveAccessComponent(player1));
        var vehicleAccessService = realmTestingServer.GetRequiredService<IVehicleAccessService>();

        var player1CanEnter = vehicleAccessService.InternalCanEnter(player1, vehicle, accessComponent);
        var player2CanEnter = vehicleAccessService.InternalCanEnter(player2, vehicle, accessComponent);
        player1CanEnter.Should().BeTrue();
        player2CanEnter.Should().BeFalse();
    }

    [Fact]
    public void VehicleNoAccessComponentShouldWork()
    {
        var realmTestingServer = new RealmTestingServer(null);
        var entityHelper = new EntityHelper(realmTestingServer);
        var entityFactory = realmTestingServer.GetRequiredService<IEntityFactory>();

        var player = entityHelper.CreatePlayerEntity();
        var vehicle = entityFactory.CreateVehicle(404, Vector3.Zero, Vector3.Zero);
        var accessComponent = vehicle.AddComponent<VehicleNoAccessComponent>();
        var vehicleAccessService = realmTestingServer.GetRequiredService<IVehicleAccessService>();

        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle, accessComponent);
        canEnter.Should().BeFalse();
    }

    [Fact]
    public void DefaultAccessComponentShouldWork()
    {
        var realmTestingServer = new RealmTestingServer(null);
        var entityHelper = new EntityHelper(realmTestingServer);
        var entityFactory = realmTestingServer.GetRequiredService<IEntityFactory>();

        var player = entityHelper.CreatePlayerEntity();
        var vehicle = entityFactory.CreateVehicle(404, Vector3.Zero, Vector3.Zero);
        var accessComponent = vehicle.AddComponent<VehicleDefaultAccessComponent>();
        var vehicleAccessService = realmTestingServer.GetRequiredService<IVehicleAccessService>();

        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle, accessComponent);
        canEnter.Should().BeTrue();
    }

    [Fact]
    public void VehicleAccessServiceCanEnterMethod()
    {
        var realmTestingServer = new RealmTestingServer(null);
        var entityHelper = new EntityHelper(realmTestingServer);
        var entityFactory = realmTestingServer.GetRequiredService<IEntityFactory>();

        var player1 = entityHelper.CreatePlayerEntity();
        var player2 = entityHelper.CreatePlayerEntity();
        var vehicle = entityFactory.CreateVehicle(404, Vector3.Zero, Vector3.Zero);
        var accessComponent = vehicle.AddComponent<VehicleNoAccessComponent>();
        var vehicleAccessService = realmTestingServer.GetRequiredService<IVehicleAccessService>();

        vehicleAccessService.CanEnter += (pedEntity, vehicleEntity) => pedEntity == player1;

        var player1CanEnter = vehicleAccessService.InternalCanEnter(player1, vehicle, accessComponent);
        var player2CanEnter = vehicleAccessService.InternalCanEnter(player2, vehicle, accessComponent);
        player1CanEnter.Should().BeTrue();
        player2CanEnter.Should().BeFalse();
    }
}
