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
        bool entityEntered = false;
        collisionSphere.Entered += (that, e) =>
        {
            entityEntered = true;
        };
        #endregion

        #region Act
        collisionSphere.CheckElementWithin(player);
        #endregion

        #region Assert
        entityEntered.Should().BeTrue();
        #endregion
    }
}
