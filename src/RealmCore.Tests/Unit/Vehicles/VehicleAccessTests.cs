﻿namespace RealmCore.Tests.Unit.Vehicles;

public class VehicleAccessTests : RealmUnitTestingBase
{
    [Fact]
    public void PlayerShouldBeAbleToEnterVehicleIfNoAccessGetsConfigured()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        var vehicle = server.CreateVehicle();

        var vehicleAccessService = server.GetRequiredService<IVehiclesAccessService>();
        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle, 0);
        canEnter.Should().BeTrue();
    }

    [Fact]
    public void VehicleExclusiveAccessControllerShouldWork()
    {
        var server = CreateServer();
        var player1 = CreatePlayer();
        var player2 = CreatePlayer();
        var vehicle = server.CreateVehicle();

        vehicle.AccessController = new VehicleExclusiveAccessController(player1);
        var vehicleAccessService = server.GetRequiredService<IVehiclesAccessService>();

        var player1CanEnter = vehicleAccessService.InternalCanEnter(player1, vehicle, 0, vehicle.AccessController);
        var player2CanEnter = vehicleAccessService.InternalCanEnter(player2, vehicle, 0, vehicle.AccessController);
        player1CanEnter.Should().BeTrue();
        player2CanEnter.Should().BeFalse();
    }

    [Fact]
    public void VehicleNoAccessControllerShouldWork()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        var vehicle = server.CreateVehicle();

        vehicle.AccessController = VehicleNoAccessController.Instance;
        var vehicleAccessService = server.GetRequiredService<IVehiclesAccessService>();

        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle, 0, vehicle.AccessController);
        canEnter.Should().BeFalse();
    }

    [Fact]
    public void DefaultAccessControllerShouldWork()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        var vehicle = server.CreateVehicle();

        vehicle.AccessController = VehicleDefaultAccessController.Instance;
        var vehicleAccessService = server.GetRequiredService<IVehiclesAccessService>();

        var canEnter = vehicleAccessService.InternalCanEnter(player, vehicle, 0, vehicle.AccessController);
        canEnter.Should().BeTrue();
    }

    [Fact]
    public void VehicleAccessServiceCanEnterMethod()
    {
        var server = CreateServer();
        var player1 = CreatePlayer();
        var player2 = CreatePlayer();
        var vehicle = server.CreateVehicle();

        vehicle.AccessController = VehicleNoAccessController.Instance;
        var vehicleAccessService = server.GetRequiredService<IVehiclesAccessService>();

        vehicleAccessService.CanEnter += (ped, vehicle, seat) => ped == player1;

        var player1CanEnter = vehicleAccessService.InternalCanEnter(player1, vehicle, 0, vehicle.AccessController);
        var player2CanEnter = vehicleAccessService.InternalCanEnter(player2, vehicle, 0, vehicle.AccessController);
        player1CanEnter.Should().BeTrue();
        player2CanEnter.Should().BeFalse();
    }
}
