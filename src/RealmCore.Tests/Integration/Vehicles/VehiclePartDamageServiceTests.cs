namespace RealmCore.Tests.Integration.Vehicles;

public class VehiclePartDamageServiceTests
{
    [Fact]
    public void AddPartShouldThrowOnDuplicatedParts()
    {
        using var hosting = new RealmTestingServerHosting();
        var vehicle = hosting.CreateVehicle();

        vehicle.PartDamage.TryAddPart(1, 100).Should().BeTrue();

        vehicle.PartDamage.HasPart(1).Should().BeTrue();
        vehicle.PartDamage.TryGetState(1, out float state1).Should().BeTrue();
        state1.Should().Be(100);
        vehicle.PartDamage.TryGetState(2, out float state2).Should().BeFalse();
        state2.Should().Be(0);

        vehicle.PartDamage.TryAddPart(1, 100).Should().BeFalse();
    }

    [Fact]
    public void AddPartShouldThrowOnNegativeState()
    {
        using var hosting = new RealmTestingServerHosting();
        var vehicle = hosting.CreateVehicle();

        var addPart = () => vehicle.PartDamage.TryAddPart(1, -100);

        addPart.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddingPartWithZeroStateShouldTriggerDestroyedEvent()
    {
        using var hosting = new RealmTestingServerHosting();
        var vehicle = hosting.CreateVehicle();

        bool destroyed = false;
        void handlePartDestroyed(VehiclePartDamageFeature arg1, short partId)
        {
            destroyed = true;
        }
        vehicle.PartDamage.PartDestroyed += handlePartDestroyed;
        vehicle.PartDamage.TryAddPart(1, 0);

        destroyed.Should().BeTrue();
    }

    [Fact]
    public void RemovePartShouldRemovePart()
    {
        using var hosting = new RealmTestingServerHosting();
        var vehicle = hosting.CreateVehicle();

        vehicle.PartDamage.TryAddPart(1, 100).Should().BeTrue();
        vehicle.PartDamage.TryRemovePart(1).Should().BeFalse();

        vehicle.PartDamage.Parts.Should().BeEmpty();
    }

    [InlineData(-50, false)]
    [InlineData(-100, true)]
    [Theory]
    public void RemovePartShouldBeRemovedWhenStateFallBelowZero(float difference, bool shouldBeDestroyed)
    {
        using var hosting = new RealmTestingServerHosting();
        var vehicle = hosting.CreateVehicle();

        bool destroyed = false;
        void handlePartDestroyed(VehiclePartDamageFeature arg1, short partId)
        {
            destroyed = true;
        }
        vehicle.PartDamage.PartDestroyed += handlePartDestroyed;
        vehicle.PartDamage.TryAddPart(1, 100);

        vehicle.PartDamage.Modify(1, difference);
        destroyed.Should().Be(shouldBeDestroyed);
    }
}
