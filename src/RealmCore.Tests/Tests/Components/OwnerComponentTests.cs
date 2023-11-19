namespace RealmCore.Tests.Tests.Components;

public class OwnerComponentTests
{
    [Fact]
    public void OwnerComponentTestsShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();

        player.AddComponent(new OwnerComponent(worldObject));
        worldObject.Destroy();

        player.Components.ComponentsList.Should().BeEmpty();
    }

    [Fact]
    public void OwnerComponentCantOwnItself()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var act = () => player.AddComponent(new OwnerComponent(player));
        act.Should().Throw<InvalidOperationException>();
        player.Components.ComponentsCount.Should().Be(0);
    }
}
