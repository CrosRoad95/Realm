namespace RealmCore.Tests.Tests.Components;

public class VehicleUpgradeComponentTests
{
    private readonly RealmTestingServer _realmTestingServer;

    public VehicleUpgradeComponentTests()
    {
        _realmTestingServer = new RealmTestingServer();

        var vehicleUpgradeRegistry = _realmTestingServer.GetRequiredService<VehicleUpgradeRegistry>();

        vehicleUpgradeRegistry.AddUpgrade(1000000, new VehicleUpgradeRegistryEntry
        {
            MaxVelocity = new FloatValueUpgradeDescription
            {
                IncreaseByUnits = 100,
                MultipleBy = 2,
            },
        });

        vehicleUpgradeRegistry.AddUpgrade(1000001, new VehicleUpgradeRegistryEntry
        {
            MaxVelocity = new FloatValueUpgradeDescription
            {
                IncreaseByUnits = 10,
                MultipleBy = 10,
            },
        });

        #region Assert default handling
        var handling = VehicleHandlingConstants.DefaultVehicleHandling[(ushort)SlipeServer.Server.Elements.VehicleModel.Perennial];
        handling.MaxVelocity.Should().Be(150);
        #endregion
    }

    [InlineData(1000000, 400)]
    [InlineData(1000001, 1510)]
    [Theory]
    public void AddUpgradeShouldWork(int upgradeId, int expectedMaxVelocity)
    {
        #region Arrange
        var vehicleEntity = _realmTestingServer.GetRequiredService<IElementFactory>().CreateVehicle(404, Vector3.Zero, Vector3.Zero);

        var vehicleUpgradesComponent = vehicleEntity.AddComponent<VehicleUpgradesComponent>();
        #endregion

        #region Act
        vehicleUpgradesComponent.AddUpgrade(upgradeId);
        #endregion

        #region Assert
        vehicleEntity.GetRequiredComponent<VehicleElementComponent>().Vehicle.Handling!.Value.MaxVelocity.Should().Be(expectedMaxVelocity);
        #endregion
    }

    [InlineData(1000000, 400)]
    [InlineData(1000001, 1510)]
    [Theory]
    public void AddUniqueUpgradeShouldAddOnlyOneInstanceOfUpgrade(int upgradeId, int expectedMaxVelocity)
    {
        #region Act
        var vehicleEntity = _realmTestingServer.GetRequiredService<IElementFactory>().CreateVehicle(404, Vector3.Zero, Vector3.Zero);
        var vehicleUpgradesComponent = vehicleEntity.AddComponent<VehicleUpgradesComponent>();
        var resultA = vehicleUpgradesComponent.AddUniqueUpgrade(upgradeId);
        var resultB = vehicleUpgradesComponent.AddUniqueUpgrade(upgradeId);
        #endregion

        #region Assert
        resultA.Should().BeTrue();
        resultB.Should().BeFalse();
        vehicleEntity.GetRequiredComponent<VehicleElementComponent>().Vehicle.Handling.Value.MaxVelocity.Should().Be(expectedMaxVelocity);
        #endregion
    }

    [Fact]
    public void UpgradesCanBeRemoved()
    {
        #region Act
        var vehicleEntity = _realmTestingServer.GetRequiredService<IElementFactory>().CreateVehicle(404, Vector3.Zero, Vector3.Zero);
        var vehicleUpgradesComponent = vehicleEntity.AddComponent<VehicleUpgradesComponent>();
        vehicleUpgradesComponent.AddUpgrade(1000000);
        vehicleUpgradesComponent.RemoveUpgrade(1000000);
        #endregion

        #region Assert
        vehicleEntity.GetRequiredComponent<VehicleElementComponent>().Vehicle.Handling.Value.MaxVelocity.Should().Be(150);
        #endregion
    }

    [Fact]
    public void MultipleUpgradesOfSameTypeCanBeAdded()
    {
        #region Act
        var vehicleEntity = _realmTestingServer.GetRequiredService<IElementFactory>().CreateVehicle(404, Vector3.Zero, Vector3.Zero);
        var vehicleUpgradesComponent = vehicleEntity.AddComponent<VehicleUpgradesComponent>();
        vehicleUpgradesComponent.AddUpgrades(Enumerable.Range(1, 3).Select(x => 1000000));
        #endregion

        #region Assert
        vehicleEntity.GetRequiredComponent<VehicleElementComponent>().Vehicle.Handling.Value.MaxVelocity.Should().Be(1200);
        #endregion
    }
}
