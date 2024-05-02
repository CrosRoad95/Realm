namespace RealmCore.Tests.Unit.Vehicles;

public class VehiclesTests : RealmUnitTestingBase
{
    [Fact]
    public void IsInMovePropertyShouldWork()
    {
        var server = CreateServer();
        var vehicle = server.CreateVehicle();

        vehicle.IsInMove.Should().BeFalse();
        vehicle.Velocity = new Vector3(0.1f, 0.1f, 0.1f);
        vehicle.IsInMove.Should().BeTrue();
    }

    [Fact]
    public void VehicleContextShouldWork()
    {
        var server = CreateServer();
        var vehicle = server.CreateVehicle();

        vehicle.GetRequiredService<VehicleContext>().Vehicle.Should().Be(vehicle);
    }

    [Fact]
    public void VehicleFuelShouldWork()
    {
        var server = CreateServer();
        var vehicle = server.CreateVehicle();

        var fuelContainer = vehicle.Fuel.AddFuelContainer(0, 50, 50, 1, 1, true);
        fuelContainer.Update(true);
        for(int i = 1; i <= 100; i++)
        {
            vehicle.Position = new Vector3(i * 10, 0, 0);
            fuelContainer.Update(true);
        }

        fuelContainer.Amount.Should().BeApproximately(49, 0.001f);
    }

    [Fact]
    public void VehicleFuelShouldNotBeConsumedWhenVehicleMovedOverLargeDistance()
    {
        var server = CreateServer();
        var vehicle = server.CreateVehicle();

        var fuelContainer = vehicle.Fuel.AddFuelContainer(0, 50, 50, 1, 1, true);
        fuelContainer.Update(true);
        vehicle.Position = new Vector3(1000, 0, 0);
        fuelContainer.Update(true);

        fuelContainer.Amount.Should().Be(50);
    }

    [Fact]
    public void VehicleEngineShouldBeTurnedOffWhenFuelRanOut()
    {
        var server = CreateServer();
        var vehicle = server.CreateVehicle();
        vehicle.IsEngineOn = true;

        var fuelContainer = vehicle.Fuel.AddFuelContainer(0, 100, 100, 100, 1, true);
        var traveledDistance = 0;
        for(int i = 1; i <= 1000; i++)
        {
            traveledDistance += 10;
            vehicle.Position = new Vector3(traveledDistance, 0, 0);
            fuelContainer.Update(true);
            if (!vehicle.IsEngineOn)
                break;
        }
        
        traveledDistance.Should().Be(1000);
    }

    [Fact]
    public void FuelShouldNotBeConsumedWhenFuelContainerIsNotActive()
    {
        var server = CreateServer();
        var vehicle = server.CreateVehicle();
        vehicle.IsEngineOn = true;

        var fuelContainer = vehicle.Fuel.AddFuelContainer(0, 100, 100, 100, 1, true);
        fuelContainer.Active = false;

        vehicle.Position = new Vector3(10, 0, 0);
        fuelContainer.Update();

        fuelContainer.Amount.Should().Be(100);
    }
}
