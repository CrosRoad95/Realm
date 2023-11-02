namespace RealmCore.Tests.Tests.Components;

public class CurrentInteractEntityComponentTests
{
    [Fact]
    public void DestroyingComponentShouldReset()
    {
        #region Arrange
        TestDateTimeProvider testDateTimeProvider = new();
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();

        var currentInteractionComponent = player.AddComponent(new CurrentInteractElementComponent(worldObject));
        currentInteractionComponent.CurrentInteractElement.Should().Be(worldObject);
        #endregion

        #region Act
        worldObject.Destroy();
        #endregion

        #region Assert
        currentInteractionComponent.CurrentInteractElement.Should().BeNull();
        #endregion
    }

    [Fact]
    public void DestroyingEntityShouldResetAndRemoveComponent()
    {
        #region Arrange
        TestDateTimeProvider testDateTimeProvider = new();
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();

        var currentInteractionComponent = player.AddComponent(new CurrentInteractElementComponent(worldObject));
        currentInteractionComponent.CurrentInteractElement.Should().Be(worldObject);
        #endregion

        #region Act
        worldObject.Destroy();
        #endregion

        #region Assert
        currentInteractionComponent.CurrentInteractElement.Should().BeNull();
        worldObject.Components.ComponentsList.Should().BeEmpty();
        #endregion
    }
}
