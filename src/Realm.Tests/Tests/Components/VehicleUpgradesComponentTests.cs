using Realm.Domain.Components.Elements;
using Realm.Domain.Components.Vehicles;
using Realm.Domain.Data.Upgrades;
using Realm.Domain.Enums;
using Realm.Domain.Interfaces;
using Realm.Domain.Registries;
using SlipeServer.Server.Constants;
using System.Numerics;

namespace Realm.Tests.Tests.Components;

public class VehicleUpgradesComponentTests
{
    private readonly Entity _entity;
    private readonly VehicleUpgradesComponent _vehicleUpgradesComponent;
    private readonly VehicleElementComponent _vehicleElementComponent;
    private readonly VehicleUpgradeRegistry _vehicleUpgradeRegistry;
    private readonly Mock<IECS> _mock = new(MockBehavior.Strict);

    public VehicleUpgradesComponentTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<VehicleUpgradeRegistry>();
        services.AddSingleton(_mock.Object);

        var serviceProvider = services.BuildServiceProvider();
        _vehicleUpgradeRegistry = serviceProvider.GetRequiredService<VehicleUpgradeRegistry>();
        _entity = new(serviceProvider, "test", EntityTag.Unknown);
        _vehicleUpgradesComponent = new();
        _vehicleElementComponent = new VehicleElementComponent(new SlipeServer.Server.Elements.Vehicle(SlipeServer.Server.Elements.VehicleModel.Perennial, Vector3.Zero));
        _entity.AddComponent(_vehicleElementComponent);
        _entity.AddComponent(_vehicleUpgradesComponent);

        _vehicleUpgradeRegistry.AddUpgrade(1, new VehicleUpgradeRegistryEntry
        {
            MaxVelocity = new FloatValueUpgradeDescription
            {
                IncreaseByUnits = 10,
                MultipleBy = 10,
            },
        });

        _vehicleUpgradeRegistry.AddUpgrade(2, new VehicleUpgradeRegistryEntry
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

    [Fact]
    public void AddUpgradeShouldWork()
    {
        #region Act
        _vehicleUpgradesComponent.RemoveAllUpgrades();
        _vehicleUpgradesComponent.AddUpgrade(1);
        #endregion

        #region Assert
        _vehicleElementComponent.Vehicle.Handling.Value.MaxVelocity.Should().Be(1510);
        #endregion
    }

    [Fact]
    public void AddUniqueUpgradeShouldAddOnlyOneInstanceOfUpgrade()
    {
        #region Act
        _vehicleUpgradesComponent.RemoveAllUpgrades();
        var resultA = _vehicleUpgradesComponent.AddUniqueUpgrade(1);
        var resultB = _vehicleUpgradesComponent.AddUniqueUpgrade(1);
        #endregion

        #region Assert
        resultA.Should().BeTrue();
        resultB.Should().BeFalse();
        _vehicleElementComponent.Vehicle.Handling.Value.MaxVelocity.Should().Be(1510);
        #endregion
    }

    [Fact]
    public void UpgradesCanBeRemoved()
    {
        #region Act
        _vehicleUpgradesComponent.RemoveAllUpgrades();
        _vehicleUpgradesComponent.AddUpgrade(1);
        _vehicleUpgradesComponent.RemoveUpgrade(1);
        #endregion

        #region Assert
        _vehicleElementComponent.Vehicle.Handling.Value.MaxVelocity.Should().Be(150);
        #endregion
    }

    [Fact]
    public void MultipleUpgradesOfSameTypeCanBeAdded()
    {
        #region Act
        _vehicleUpgradesComponent.RemoveAllUpgrades();
        _vehicleUpgradesComponent.AddUpgrades(Enumerable.Range(1, 3).Select(x => 1));
        #endregion

        #region Assert
        _vehicleElementComponent.Vehicle.Handling.Value.MaxVelocity.Should().Be(4530);
        #endregion
    }
}
