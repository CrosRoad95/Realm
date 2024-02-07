using SlipeServer.Packets.Definitions.Entities.Structs;

namespace RealmCore.Tests.Tests;

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

public class VehicleUpgradesTests
{
    private readonly RealmTestingServer _realmTestingServer;

    public VehicleUpgradesTests()
    {
        _realmTestingServer = new RealmTestingServer();

        var vehicleUpgradeRegistry = _realmTestingServer.GetRequiredService<VehicleUpgradeRegistry>();

        vehicleUpgradeRegistry.AddUpgrade(1000000, new VehicleUpgradeRegistryEntry(new VehicleUpgrade1()));
        vehicleUpgradeRegistry.AddUpgrade(1000001, new VehicleUpgradeRegistryEntry(new VehicleUpgrade2()));
        vehicleUpgradeRegistry.AddUpgrade(1000002, new VehicleUpgradeRegistryEntry(new VehicleUpgrade3()));

        #region Assert default handling
        var handling = VehicleHandlingConstants.DefaultVehicleHandling[(ushort)VehicleModel.Perennial];
        handling.MaxVelocity.Should().Be(150);
        #endregion
    }

    [InlineData(1000000, 400)]
    [InlineData(1000001, 1510)]
    [Theory]
    public void AddUpgradeShouldWork(int upgradeId, int expectedMaxVelocity)
    {
        #region Arrange
        var vehicle = _realmTestingServer.CreateVehicle();
        #endregion

        #region Act
        vehicle.Upgrades.AddUpgrade(upgradeId);
        #endregion

        #region Assert
        vehicle.Handling!.Value.MaxVelocity.Should().Be(expectedMaxVelocity);
        #endregion
    }

    [InlineData(1000000, 400)]
    [InlineData(1000001, 1510)]
    [Theory]
    public void AddUniqueUpgradeShouldAddOnlyOneInstanceOfUpgrade(int upgradeId, int expectedMaxVelocity)
    {
        #region Act
        var vehicle = _realmTestingServer.CreateVehicle();
        var resultA = vehicle.Upgrades.AddUniqueUpgrade(upgradeId);
        var resultB = vehicle.Upgrades.AddUniqueUpgrade(upgradeId);
        #endregion

        #region Assert
        resultA.Should().BeTrue();
        resultB.Should().BeFalse();
        vehicle.Handling.Value.MaxVelocity.Should().Be(expectedMaxVelocity);
        #endregion
    }

    [Fact]
    public void UpgradesCanBeRemoved()
    {
        #region Act
        var vehicle = _realmTestingServer.CreateVehicle();
        vehicle.Upgrades.AddUpgrade(1000000);
        vehicle.Upgrades.RemoveUpgrade(1000000);
        #endregion

        #region Assert
        vehicle.Handling.Value.MaxVelocity.Should().Be(150);
        #endregion
    }

    [Fact]
    public void MultipleUpgradesOfSameTypeCanBeAdded()
    {
        #region Act
        var vehicle = _realmTestingServer.CreateVehicle();
        vehicle.Upgrades.AddUpgrades(Enumerable.Range(1, 3).Select(x => 1000000));
        #endregion

        #region Assert
        // 150 - base
        // ((150 * 2 + 100) * 2 + 100) * 2 + 100
        vehicle.Handling.Value.MaxVelocity.Should().Be(1900);
        #endregion
    }

    [InlineData(new int[] { 1000002, 1000000, 1000000, 1000000 })]
    [InlineData(new int[] { 1000000, 1000002, 1000000, 1000000 })]
    [InlineData(new int[] { 1000000, 1000000, 1000002, 1000000 })]
    [InlineData(new int[] { 1000000, 1000000, 1000000, 1000002 })]
    [Theory]
    public void UpgradesMiddlewareShouldBeOrderIndependent(int[] upgrades)
    {
        #region Act
        var vehicle = _realmTestingServer.CreateVehicle();
        vehicle.Upgrades.AddUpgrades(upgrades);
        #endregion

        #region Assert
        // 150 - base
        // (((150 * 2 + 100) * 2 + 100) * 2 + 100) / 2
        vehicle.Handling.Value.MaxVelocity.Should().Be(950);
        #endregion
    }
}
