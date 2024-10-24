namespace RealmCore.Tests.Integration.Vehicles;

public class VehiclesServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly GroupsService _groupsService;
    private readonly VehiclesService _vehiclesService;

    public VehiclesServiceTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _groupsService = _fixture.Hosting.GetRequiredService<GroupsService>();
        _vehiclesService = _fixture.Hosting.GetRequiredService<VehiclesService>();
    }

    [Fact]
    public async Task YouShouldBeAbleToGetAllVehiclesAssignedToGroup()
    {
        var vehicle = await _fixture.Hosting.CreatePersistentVehicle();
        var group = await _groupsService.Create(Guid.NewGuid().ToString());
        var added = await _vehiclesService.TryAddGroupAccess(vehicle.Persistence.Id, group!.Id, 1);

        var vehicles1 = await _vehiclesService.GetVehiclesByGroupId(group!.Id);
        var vehicles2 = await _vehiclesService.GetVehiclesByGroupId(group!.Id, [2]);
        var vehicles3 = await _vehiclesService.GetVehiclesByGroupId(group!.Id, [1, 2]);

        using var _ = new AssertionScope();

        added.Should().NotBeNull();
        vehicles1.Select(x => x.Id).Should().BeEquivalentTo([vehicle.Persistence.Id]);
        vehicles2.Select(x => x.Id).Should().BeEmpty();
        vehicles3.Select(x => x.Id).Should().BeEquivalentTo([vehicle.Persistence.Id]);
    }
}
