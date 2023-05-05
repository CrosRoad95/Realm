using RealmCore.Server.Components.Vehicles;

namespace RealmCore.Tests.Tests.Components;

public class VehiclePartDamageComponentTests
{
    private readonly Entity _entity;
    private readonly VehiclePartDamageComponent _vehiclePartDamageComponent;

    public VehiclePartDamageComponentTests()
    {
        var services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        _entity = new(serviceProvider, "test", EntityTag.Unknown);
        _vehiclePartDamageComponent = new();
        _entity.AddComponent(_vehiclePartDamageComponent);
    }

    [Fact]
    public void AddPartShouldThrowOnDuplicatedParts()
    {
        var addPart = () => _vehiclePartDamageComponent.AddPart(1, 100);

        addPart.Should().NotThrow();
        addPart.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemovePartShouldRemovePart()
    {
        _vehiclePartDamageComponent.AddPart(1, 100);
        _vehiclePartDamageComponent.RemovePart(1);

        _vehiclePartDamageComponent.Parts.Should().BeEmpty();
    }
}
