namespace RealmCore.Tests.Integration.Vehicles;

public class VehiclePartDamageServiceTests : RealmUnitTestingBase
{
    [Fact]
    public void AddPartShouldThrowOnDuplicatedParts()
    {
        var server = CreateServer();
        var vehicle = server.CreateVehicle();

        var addPart = () => vehicle.PartDamage.AddPart(1, 100);

        addPart.Should().NotThrow();

        vehicle.PartDamage.HasPart(1).Should().BeTrue();
        vehicle.PartDamage.TryGetState(1, out float state1).Should().BeTrue();
        state1.Should().Be(100);
        vehicle.PartDamage.TryGetState(2, out float state2).Should().BeFalse();
        state2.Should().Be(0);

        addPart.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddPartShouldThrowOnNegativeState()
    {
        var server = CreateServer();
        var vehicle = server.CreateVehicle();

        var addPart = () => vehicle.PartDamage.AddPart(1, -100);

        addPart.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddingPartWithZeroStateShouldTriggerDestroyedEvent()
    {
        var server = CreateServer();
        var vehicle = server.CreateVehicle();

        bool destroyed = false;
        void handlePartDestroyed(IVehiclePartDamageFeature arg1, short partId)
        {
            destroyed = true;
        }
        vehicle.PartDamage.PartDestroyed += handlePartDestroyed;
        vehicle.PartDamage.AddPart(1, 0);

        destroyed.Should().BeTrue();
    }

    [Fact]
    public void RemovePartShouldRemovePart()
    {
        var server = CreateServer();
        var vehicle = server.CreateVehicle();

        vehicle.PartDamage.AddPart(1, 100);
        vehicle.PartDamage.RemovePart(1);

        vehicle.PartDamage.Parts.Should().BeEmpty();
    }

    [InlineData(-50, false)]
    [InlineData(-100, true)]
    [Theory]
    public void RemovePartShouldBeRemovedWhenStateFallBelowZero(float difference, bool shouldBeDestroyed)
    {
        var server = CreateServer();
        var vehicle = server.CreateVehicle();

        bool destroyed = false;
        void handlePartDestroyed(IVehiclePartDamageFeature arg1, short partId)
        {
            destroyed = true;
        }
        vehicle.PartDamage.PartDestroyed += handlePartDestroyed;
        vehicle.PartDamage.AddPart(1, 100);

        vehicle.PartDamage.Modify(1, difference);
        destroyed.Should().Be(shouldBeDestroyed);
    }
}
