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
}
