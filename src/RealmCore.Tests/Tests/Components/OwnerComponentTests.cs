namespace RealmCore.Tests.Tests.Components;

public class OwnerComponentTests
{
    [Fact]
    public void OwnerComponentTestsShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();

        player1.AddComponent(new OwnerComponent(worldObject));
        worldObject.Destroy();

        player1.Components.ComponentsList.Should().BeEmpty();
    }
}
