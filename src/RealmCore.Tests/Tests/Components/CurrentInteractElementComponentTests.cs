namespace RealmCore.Tests.Tests.Components;

public class CurrentInteractElementComponentTests
{
    [Fact]
    public void DestroyingComponentShouldReset()
    {
        #region Arrange
        TestDateTimeProvider testDateTimeProvider = new();
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();

        player.CurrentInteractElement = worldObject;
        player.CurrentInteractElement.Should().Be(worldObject);
        #endregion

        #region Act
        worldObject.Destroy();
        #endregion

        #region Assert
        player.CurrentInteractElement.Should().BeNull();
        #endregion
    }

    [Fact]
    public void DestroyingElementShouldResetAndRemoveComponent()
    {
        #region Arrange
        TestDateTimeProvider testDateTimeProvider = new();
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();

        player.CurrentInteractElement = worldObject;
        player.CurrentInteractElement.Should().Be(worldObject);
        #endregion

        #region Act
        worldObject.Destroy();
        #endregion

        #region Assert
        player.CurrentInteractElement.Should().BeNull();
        worldObject.Components.ComponentsList.Should().BeEmpty();
        #endregion
    }
}
