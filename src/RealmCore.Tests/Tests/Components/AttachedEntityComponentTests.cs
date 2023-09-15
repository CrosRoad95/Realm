namespace RealmCore.Tests.Tests.Components;

public class AttachedEntityComponentTests
{
    private readonly RealmTestingServer _server;
    private readonly EntityHelper _entityHelper;

    public AttachedEntityComponentTests()
    {
        _server = new();
        _entityHelper = new(_server);
    }

    [Fact]
    public void YouShouldBeAbleAttachObjectToPlayerEntity()
    {
        #region Arrange

        var playerEntity = _entityHelper.CreatePlayerEntity();
        var elementEntity = _entityHelper.CreateObjectEntity();
        #endregion

        #region Act
        var attachedEntityComponent = playerEntity.AddComponent(new AttachedEntityComponent(elementEntity, SlipeServer.Packets.Enums.BoneId.Pelvis, null, null));
        #endregion

        #region Assert
        attachedEntityComponent.AttachedEntity.Should().Be(elementEntity);
        #endregion
    }

    [Fact]
    public void AttachedEntityComponentShouldBeRemovedIfEntityDisposed()
    {
        #region Arrange

        var playerEntity = _entityHelper.CreatePlayerEntity();
        var elementEntity = _entityHelper.CreateObjectEntity();
        #endregion

        #region Act
        var attachedEntityComponent = playerEntity.AddComponent(new AttachedEntityComponent(elementEntity, SlipeServer.Packets.Enums.BoneId.Pelvis, null, null));
        elementEntity.Dispose();
        #endregion

        #region Assert
        var tryAccessAttachedEntity = () => attachedEntityComponent.AttachedEntity;
        tryAccessAttachedEntity.Should().Throw<ObjectDisposedException>();
        playerEntity.Components.Should().NotContain(attachedEntityComponent);
        #endregion
    }
}
