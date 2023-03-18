using Realm.Domain.Components.Elements;
using Realm.Domain.Components.Vehicles;
using Realm.Domain.Interfaces;
using Realm.Domain.Registries;
using SlipeServer.Server.Constants;
using System.Numerics;

namespace Realm.Tests.Tests.Components;

public class VehicleEngineComponentTests
{
    private readonly Entity _entity;
    private readonly VehicleUpgradesComponent _vehicleUpgradesComponent;
    private readonly VehicleEngineComponent _vehicleEngineComponent;
    private readonly VehicleElementComponent _vehicleElementComponent;
    private readonly VehicleUpgradeRegistry _vehicleUpgradeRegistry;
    private readonly Mock<IEntityByElement> _mock = new(MockBehavior.Strict);

    public VehicleEngineComponentTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<VehicleUpgradeRegistry>();
        services.AddSingleton<VehicleEnginesRegistry>();
        services.AddSingleton(_mock.Object);

        var serviceProvider = services.BuildServiceProvider();
        _vehicleUpgradeRegistry = serviceProvider.GetRequiredService<VehicleUpgradeRegistry>();
        var vehicleEnginesRegistry = serviceProvider.GetRequiredService<VehicleEnginesRegistry>();
        _entity = new(serviceProvider, "test", Entity.EntityTag.Unknown);
        _vehicleUpgradesComponent = new();
        _vehicleEngineComponent = new();
        _vehicleElementComponent = new VehicleElementComponent(new SlipeServer.Server.Elements.Vehicle(SlipeServer.Server.Elements.VehicleModel.Perennial, Vector3.Zero));
        _entity.AddComponent(_vehicleElementComponent);
        _entity.AddComponent(_vehicleEngineComponent);
        _entity.AddComponent(_vehicleUpgradesComponent);

        vehicleEnginesRegistry.Add(1, new VehicleEngineRegistryEntry
        {
            UpgradeId = 1,
        });

        _vehicleUpgradeRegistry.AddUpgrade(1, new VehicleUpgradeRegistryEntry
        {
            MaxVelocity = new Domain.Data.FloatValueUpgradeDescription
            {
                IncreaseByUnits = 100,
                MultipleBy = 2,
            },
        });

        _vehicleUpgradeRegistry.AddUpgrade(2, new VehicleUpgradeRegistryEntry
        {
            MaxVelocity = new Domain.Data.FloatValueUpgradeDescription
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
        _vehicleUpgradesComponent.AddUpgrade(2);
        #endregion

        #region Assert
        _vehicleElementComponent.Vehicle.Handling.Value.MaxVelocity.Should().Be(3120);
        #endregion
    }
}
