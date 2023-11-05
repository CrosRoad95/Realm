namespace RealmCore.Tests.Tests.Components;

public class CollisionShapeElementComponentTests
{
    [Fact]
    public void TestBasicCollisionDetection()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var elementFactory = realmTestingServer.GetRequiredService<IElementFactory>();
        var collisionSphere  = elementFactory.CreateCollisionSphere(Vector3.Zero, 10);
        bool elementEntered = false;
        collisionSphere.CollisionDetection.Entered += (that, e) =>
        {
            elementEntered = true;
        };
        #endregion

        #region Act
        collisionSphere.CheckElementWithin(player);
        #endregion

        #region Assert
        elementEntered.Should().BeTrue();
        #endregion
    }
}
