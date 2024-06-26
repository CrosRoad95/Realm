﻿using SlipeServer.Packets.Definitions.Entities.Structs;

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

public class VehicleUpgradesTests
{
    private void Seed(RealmTestingServerHosting hosting)
    {
        var vehicleUpgradesCollection = hosting.GetRequiredService<VehicleUpgradesCollection>();

        vehicleUpgradesCollection.Add(1000000, new VehicleUpgradesCollectionItem(new VehicleUpgrade1()));
        vehicleUpgradesCollection.Add(1000001, new VehicleUpgradesCollectionItem(new VehicleUpgrade2()));
        vehicleUpgradesCollection.Add(1000002, new VehicleUpgradesCollectionItem(new VehicleUpgrade3()));

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
        using var hosting = new RealmTestingServerHosting();

        Seed(hosting);
        var vehicle = hosting.CreateVehicle();
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
        using var hosting = new RealmTestingServerHosting();

        Seed(hosting);
        var vehicle = hosting.CreateVehicle();
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
        using var hosting = new RealmTestingServerHosting();

        Seed(hosting);
        var vehicle = hosting.CreateVehicle();
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
        using var hosting = new RealmTestingServerHosting();

        Seed(hosting);
        var vehicle = hosting.CreateVehicle();
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
        using var hosting = new RealmTestingServerHosting();

        Seed(hosting);
        var vehicle = hosting.CreateVehicle();
        vehicle.Upgrades.AddUpgrades(upgrades);
        #endregion

        #region Assert
        // 150 - base
        // (((150 * 2 + 100) * 2 + 100) * 2 + 100) / 2
        vehicle.Handling.Value.MaxVelocity.Should().Be(950);
        #endregion
    }
}
