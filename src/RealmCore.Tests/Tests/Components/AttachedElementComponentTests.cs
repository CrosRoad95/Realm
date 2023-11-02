namespace RealmCore.Tests.Tests.Components;

public class AttachedElementComponentTests
{
    [Fact]
    public void YouShouldBeAbleAttachObjectToPlayer()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
        #endregion

        #region Act
        var attachedElementComponent = player.AddComponent(new AttachedElementComponent(worldObject, SlipeServer.Packets.Enums.BoneId.Pelvis, null, null));
        #endregion

        #region Assert
        attachedElementComponent.AttachedElement.Should().Be(worldObject);
        #endregion
    }

    [Fact]
    public void AttachedElementComponentShouldBeRemovedIfElementGetsDestroyed()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
        #endregion

        #region Act
        var attachedElementComponent = player.AddComponent(new AttachedElementComponent(worldObject, SlipeServer.Packets.Enums.BoneId.Pelvis, null, null));
        worldObject.Destroy();
        #endregion

        #region Assert
        attachedElementComponent.AttachedElement.Should().BeNull();
        player.Components.ComponentsList.Should().BeEmpty();
        attachedElementComponent.Element.Should().BeNull();
        #endregion
    }
}
