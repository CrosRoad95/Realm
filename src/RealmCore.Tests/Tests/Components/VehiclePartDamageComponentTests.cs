namespace RealmCore.Tests.Tests.Components;

public class VehiclePartDamageComponentTests
{
    [Fact]
    public void AddPartShouldThrowOnDuplicatedParts()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var vehiclePartDamageComponent = player.AddComponent<VehiclePartDamageComponent>();

        var addPart = () => vehiclePartDamageComponent.AddPart(1, 100);

        addPart.Should().NotThrow();
        addPart.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemovePartShouldRemovePart()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var vehiclePartDamageComponent = player.AddComponent<VehiclePartDamageComponent>();

        vehiclePartDamageComponent.AddPart(1, 100);
        vehiclePartDamageComponent.RemovePart(1);

        vehiclePartDamageComponent.Parts.Should().BeEmpty();
    }
}
