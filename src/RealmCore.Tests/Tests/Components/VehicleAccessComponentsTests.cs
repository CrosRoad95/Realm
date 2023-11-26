using RealmCore.Server.Components.Vehicles.Access;

namespace RealmCore.Tests.Tests.Components;

public class VehicleAccessComponentsTests
{
    [Fact]
    public void PlayerShouldBeAbleToEnterVehicleIfNoAccessGetsConfigured()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var vehicle = realmTestingServer.CreateVehicle();

        var vehicleAccessService = realmTestingServer.GetRequiredService<IVehiclesAccessService>();
        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle, 0);
        canEnter.Should().BeTrue();
    }

    [Fact]
    public void VehicleExclusiveAccessComponentShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var vehicle = realmTestingServer.CreateVehicle();

        var accessComponent = vehicle.AddComponent(new VehicleExclusiveAccessComponent(player1));
        var vehicleAccessService = realmTestingServer.GetRequiredService<IVehiclesAccessService>();

        var player1CanEnter = vehicleAccessService.InternalCanEnter(player1, vehicle, 0, accessComponent);
        var player2CanEnter = vehicleAccessService.InternalCanEnter(player2, vehicle, 0, accessComponent);
        player1CanEnter.Should().BeTrue();
        player2CanEnter.Should().BeFalse();
    }

    [Fact]
    public void VehicleNoAccessComponentShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var vehicle = realmTestingServer.CreateVehicle();

        var accessComponent = vehicle.AddComponent<VehicleNoAccessComponent>();
        var vehicleAccessService = realmTestingServer.GetRequiredService<IVehiclesAccessService>();

        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle, 0, accessComponent);
        canEnter.Should().BeFalse();
    }

    [Fact]
    public void DefaultAccessComponentShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var vehicle = realmTestingServer.CreateVehicle();

        var accessComponent = vehicle.AddComponent<VehicleDefaultAccessComponent>();
        var vehicleAccessService = realmTestingServer.GetRequiredService<IVehiclesAccessService>();

        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle, 0, accessComponent);
        canEnter.Should().BeTrue();
    }

    [Fact]
    public void VehicleAccessServiceCanEnterMethod()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var vehicle = realmTestingServer.CreateVehicle();

        var accessComponent = vehicle.AddComponent<VehicleNoAccessComponent>();
        var vehicleAccessService = realmTestingServer.GetRequiredService<IVehiclesAccessService>();

        vehicleAccessService.CanEnter += (ped, vehicle, seat) => ped == player1;

        var player1CanEnter = vehicleAccessService.InternalCanEnter(player1, vehicle, 0, accessComponent);
        var player2CanEnter = vehicleAccessService.InternalCanEnter(player2, vehicle, 0, accessComponent);
        player1CanEnter.Should().BeTrue();
        player2CanEnter.Should().BeFalse();
    }
}
