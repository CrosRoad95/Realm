namespace RealmCore.Tests.Integration.Vehicles;

public class VehiclePartDamageServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;

    public VehiclePartDamageServiceTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
    }

    [Fact]
    public void AddPartShouldThrowOnDuplicatedParts()
    {
        var vehicle = _fixture.Hosting.CreateVehicle();

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
        var vehicle = _fixture.Hosting.CreateVehicle();

        var addPart = () => vehicle.PartDamage.TryAddPart(1, -100);

        addPart.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddingPartWithZeroStateShouldTriggerDestroyedEvent()
    {
        var vehicle = _fixture.Hosting.CreateVehicle();

        bool destroyed = false;
        void handlePartDestroyed(VehiclePartDamageFeature arg1, short partId)
        {
            destroyed = true;
        }
        vehicle.PartDamage.PartRemoved += handlePartDestroyed;

        vehicle.PartDamage.TryAddPart(1, 10);
        vehicle.PartDamage.TryModify(1, -10);

        destroyed.Should().BeTrue();
    }

    [Fact]
    public void RemovePartShouldRemovePart()
    {
        var vehicle = _fixture.Hosting.CreateVehicle();

        vehicle.PartDamage.TryAddPart(1, 100).Should().BeTrue();
        vehicle.PartDamage.TryRemovePart(1).Should().BeTrue();

        vehicle.PartDamage.Parts.Should().BeEmpty();
    }

    [InlineData(-50, false)]
    [InlineData(-100, true)]
    [Theory]
    public void RemovePartShouldBeRemovedWhenStateFallBelowZero(float difference, bool shouldBeDestroyed)
    {
        var vehicle = _fixture.Hosting.CreateVehicle();

        bool destroyed = false;
        void handlePartDestroyed(VehiclePartDamageFeature arg1, short partId)
        {
            destroyed = true;
        }
        vehicle.PartDamage.PartRemoved += handlePartDestroyed;
        vehicle.PartDamage.TryAddPart(1, 100);

        vehicle.PartDamage.TryModify(1, difference);
        destroyed.Should().Be(shouldBeDestroyed);
    }
}
