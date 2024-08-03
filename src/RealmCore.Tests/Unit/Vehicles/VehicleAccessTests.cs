namespace RealmCore.Tests.Unit.Vehicles;

public class VehicleAccessTests
{
    [Fact]
    public async Task PlayerShouldBeAbleToEnterVehicleIfNoAccessGetsConfigured()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var vehicle = hosting.CreateVehicle();

        var vehicleAccessService = hosting.GetRequiredService<VehiclesAccessService>();
        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle, 0);
        canEnter.Should().BeTrue();
    }

    [Fact]
    public async Task VehicleExclusiveAccessControllerShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player1 = await hosting.CreatePlayer();
        var player2 = await hosting.CreatePlayer();
        var vehicle = hosting.CreateVehicle();

        vehicle.AccessController = new VehicleExclusiveAccessController(player1);
        var vehicleAccessService = hosting.GetRequiredService<VehiclesAccessService>();

        var player1CanEnter = vehicleAccessService.InternalCanEnter(player1, vehicle, 0, vehicle.AccessController);
        var player2CanEnter = vehicleAccessService.InternalCanEnter(player2, vehicle, 0, vehicle.AccessController);
        player1CanEnter.Should().BeTrue();
        player2CanEnter.Should().BeFalse();
    }

    [Fact]
    public async Task VehicleNoAccessControllerShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var vehicle = hosting.CreateVehicle();

        vehicle.AccessController = VehicleNoAccessController.Instance;
        var vehicleAccessService = hosting.GetRequiredService<VehiclesAccessService>();

        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle, 0, vehicle.AccessController);
        canEnter.Should().BeFalse();
    }

    [Fact]
    public async Task DefaultAccessControllerShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var vehicle = hosting.CreateVehicle();

        vehicle.AccessController = VehicleDefaultAccessController.Instance;
        var vehicleAccessService = hosting.GetRequiredService<VehiclesAccessService>();

        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle, 0, vehicle.AccessController);
        canEnter.Should().BeTrue();
    }

    [Fact]
    public async Task VehicleAccessServiceCanEnterMethod()
    {
        using var hosting = new RealmTestingServerHosting();
        var player1 = await hosting.CreatePlayer();
        var player2 = await hosting.CreatePlayer();
        var vehicle = hosting.CreateVehicle();

        vehicle.AccessController = VehicleNoAccessController.Instance;
        var vehicleAccessService = hosting.GetRequiredService<VehiclesAccessService>();

        vehicleAccessService.CanEnter += (ped, vehicle, seat) => ped == player1;

        var player1CanEnter = vehicleAccessService.InternalCanEnter(player1, vehicle, 0, vehicle.AccessController);
        var player2CanEnter = vehicleAccessService.InternalCanEnter(player2, vehicle, 0, vehicle.AccessController);
        player1CanEnter.Should().BeTrue();
        player2CanEnter.Should().BeFalse();
    }
}
