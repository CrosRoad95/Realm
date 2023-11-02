namespace RealmCore.Tests.Tests.Components;

public class AttachedEntityComponentTests
{
    [Fact]
    public void YouShouldBeAbleAttachObjectToPlayerEntity()
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
    public void AttachedEntityComponentShouldBeRemovedIfEntityDisposed()
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
        var tryAccessAttachedEntity = () => attachedElementComponent.AttachedElement;
        tryAccessAttachedEntity.Should().NotThrow<ObjectDisposedException>();
        player.Components.ComponentsList.Should().BeEmpty();
        attachedElementComponent.Element.Should().BeNull();
        #endregion
    }
}
