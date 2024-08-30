using SlipeServer.Packets.Definitions.Entities.Structs;

namespace RealmCore.Tests.Unit.Vehicles;

public class VehicleUpgrade1 : IVehicleHandlingModifier
{
    public void Apply(VehicleHandlingContext context, HandlingDelegate next)
    {
        context.Modify((ref VehicleHandling vehicleHandling) =>
        {
            vehicleHandling.MaxVelocity *= 2;
            vehicleHandling.MaxVelocity += 100;
        });
        next(context);
    }
}

public class VehicleUpgrade2 : IVehicleHandlingModifier
{
    public void Apply(VehicleHandlingContext context, HandlingDelegate next)
    {
        context.Modify((ref VehicleHandling vehicleHandling) =>
        {
            vehicleHandling.MaxVelocity *= 10;
            vehicleHandling.MaxVelocity += 10;
        });
        next(context);
    }
}

public class VehicleUpgrade3 : IVehicleHandlingModifier
{
    public void Apply(VehicleHandlingContext context, HandlingDelegate next)
    {
        next(context);
        context.Modify((ref VehicleHandling vehicleHandling) =>
        {
            vehicleHandling.MaxVelocity /= 2;
        });
    }
}

public class VehicleUpgradesTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;

    public VehicleUpgradesTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
        Seed(_hosting.GetRequiredService<VehicleUpgradesCollection>());
    }

    private void Seed(VehicleUpgradesCollection vehicleUpgradesCollection)
    {
        if (vehicleUpgradesCollection.HasKey(1000000))
            return;

        vehicleUpgradesCollection.Add(1000000, new VehicleUpgradesCollectionItem(new VehicleUpgrade1()));
        vehicleUpgradesCollection.Add(1000001, new VehicleUpgradesCollectionItem(new VehicleUpgrade2()));
        vehicleUpgradesCollection.Add(1000002, new VehicleUpgradesCollectionItem(new VehicleUpgrade3()));

        var handling = VehicleHandlingConstants.DefaultVehicleHandling[(ushort)VehicleModel.Perennial];
        handling.MaxVelocity.Should().Be(150);
    }

    [InlineData(1000000, 400)]
    [InlineData(1000001, 1510)]
    [Theory]
    public void AddUpgradeShouldWork(int upgradeId, int expectedMaxVelocity)
    {
        var vehicle = _hosting.CreateVehicle();

        vehicle.Upgrades.AddUpgrade(upgradeId);

        vehicle.Handling!.Value.MaxVelocity.Should().Be(expectedMaxVelocity);
    }

    [InlineData(1000000, 400)]
    [InlineData(1000001, 1510)]
    [Theory]
    public void AddUniqueUpgradeShouldAddOnlyOneInstanceOfUpgrade(int upgradeId, int expectedMaxVelocity)
    {
        var vehicle = _hosting.CreateVehicle();
        var resultA = vehicle.Upgrades.AddUniqueUpgrade(upgradeId);
        var resultB = vehicle.Upgrades.AddUniqueUpgrade(upgradeId);
        resultA.Should().BeTrue();
        resultB.Should().BeFalse();
        vehicle.Handling.Value.MaxVelocity.Should().Be(expectedMaxVelocity);
    }

    [Fact]
    public void UpgradesCanBeRemoved()
    {
        var vehicle = _hosting.CreateVehicle();
        vehicle.Upgrades.AddUpgrade(1000000);
        vehicle.Upgrades.RemoveUpgrade(1000000);

        vehicle.Handling.Value.MaxVelocity.Should().Be(150);
    }

    [Fact]
    public void MultipleUpgradesOfSameTypeCanBeAdded()
    {
        var vehicle = _hosting.CreateVehicle();
        vehicle.Upgrades.AddUpgrades(Enumerable.Range(1, 3).Select(x => 1000000));

        // 150 - base
        // ((150 * 2 + 100) * 2 + 100) * 2 + 100
        vehicle.Handling.Value.MaxVelocity.Should().Be(1900);
    }

    [InlineData(new int[] { 1000002, 1000000, 1000000, 1000000 })]
    [InlineData(new int[] { 1000000, 1000002, 1000000, 1000000 })]
    [InlineData(new int[] { 1000000, 1000000, 1000002, 1000000 })]
    [InlineData(new int[] { 1000000, 1000000, 1000000, 1000002 })]
    [Theory]
    public void UpgradesMiddlewareShouldBeOrderIndependent(int[] upgrades)
    {
        var vehicle = _hosting.CreateVehicle();
        vehicle.Upgrades.AddUpgrades(upgrades);

        // 150 - base
        // (((150 * 2 + 100) * 2 + 100) * 2 + 100) / 2
        vehicle.Handling.Value.MaxVelocity.Should().Be(950);
    }
}
