namespace RealmCore.Tests.Unit.Vehicles;

public class VehicleAccessTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;

    public VehicleAccessTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
    }

    [Fact]
    public void PlayerShouldBeAbleToEnterVehicleIfNoAccessGetsConfigured()
    {
        var vehicle = _hosting.CreateVehicle();

        var vehicleAccessService = _hosting.GetRequiredService<VehiclesAccessService>();
        var canEnter = vehicleAccessService.InternalCanEnter(_player, vehicle, 0);
        canEnter.Should().BeTrue();
    }

    [Fact]
    public async Task VehicleExclusiveAccessControllerShouldWork()
    {
        var otherPlayer = await _hosting.CreatePlayer();
        var vehicle = _hosting.CreateVehicle();

        vehicle.AccessController = new VehicleExclusiveAccessController(_player);
        var vehicleAccessService = _hosting.GetRequiredService<VehiclesAccessService>();

        var player1CanEnter = vehicleAccessService.InternalCanEnter(_player, vehicle, 0, vehicle.AccessController);
        var player2CanEnter = vehicleAccessService.InternalCanEnter(otherPlayer, vehicle, 0, vehicle.AccessController);
        player1CanEnter.Should().BeTrue();
        player2CanEnter.Should().BeFalse();
    }

    [Fact]
    public void VehicleNoAccessControllerShouldWork()
    {
        var vehicle = _hosting.CreateVehicle();

        vehicle.AccessController = VehicleNoAccessController.Instance;
        var vehicleAccessService = _hosting.GetRequiredService<VehiclesAccessService>();

        var canEnter = vehicleAccessService.InternalCanEnter(_player, vehicle, 0, vehicle.AccessController);
        canEnter.Should().BeFalse();
    }

    [Fact]
    public void DefaultAccessControllerShouldWork()
    {
        var vehicle = _hosting.CreateVehicle();

        vehicle.AccessController = VehicleDefaultAccessController.Instance;
        var vehicleAccessService = _hosting.GetRequiredService<VehiclesAccessService>();

        var canEnter = vehicleAccessService.InternalCanEnter(_player, vehicle, 0, vehicle.AccessController);
        canEnter.Should().BeTrue();
    }

    [Fact]
    public async Task VehicleAccessServiceCanEnterMethod()
    {
        var otherPlayer = await _hosting.CreatePlayer();
        var vehicle = _hosting.CreateVehicle();

        vehicle.AccessController = VehicleNoAccessController.Instance;
        var vehicleAccessService = _hosting.GetRequiredService<VehiclesAccessService>();

        vehicleAccessService.CanEnter += (ped, vehicle, seat) => ped == _player;

        var player1CanEnter = vehicleAccessService.InternalCanEnter(_player, vehicle, 0, vehicle.AccessController);
        var player2CanEnter = vehicleAccessService.InternalCanEnter(otherPlayer, vehicle, 0, vehicle.AccessController);
        player1CanEnter.Should().BeTrue();
        player2CanEnter.Should().BeFalse();
    }
}
