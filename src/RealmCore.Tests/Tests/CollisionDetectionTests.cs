using System.Drawing;

namespace RealmCore.Tests.Tests;

public class CollisionDetectionTests
{
    [Fact]
    public void CollisionSphereDetection()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var elementFactory = realmTestingServer.GetRequiredService<IElementFactory>();
        var collisionSphere = elementFactory.CreateCollisionSphere(Vector3.Zero, 10);
        int elementEntered = 0;
        int elementLeft = 0;
        collisionSphere.CollisionDetection.Entered += (that, e) =>
        {
            elementEntered++;
        };
        collisionSphere.CollisionDetection.Left += (that, e) =>
        {
            elementLeft++;
        };
        #endregion

        #region Act
        collisionSphere.CheckElementWithin(player);
        player.Position = new Vector3(100, 0, 0);
        collisionSphere.CheckElementWithin(player);
        #endregion

        #region Assert
        elementEntered.Should().Be(1);
        elementLeft.Should().Be(1);
        #endregion
    }

    [Fact]
    public void MarkerDetection()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var elementFactory = realmTestingServer.GetRequiredService<IElementFactory>();
        var marker = elementFactory.CreateMarker(Vector3.Zero, MarkerType.Cylinder, 1, Color.Red);
        int elementEntered = 0;
        int elementLeft = 0;
        marker.CollisionDetection.Entered += (that, e) =>
        {
            elementEntered++;
        };
        marker.CollisionDetection.Left += (that, e) =>
        {
            elementLeft++;
        };
        #endregion

        #region Act
        marker.CheckElementWithin(player);
        player.Position = new Vector3(100, 0, 0);
        marker.CheckElementWithin(player);
        #endregion

        #region Assert
        elementEntered.Should().Be(1);
        elementLeft.Should().Be(1);
        #endregion
    }

    [Fact]
    public void PickupDetection()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var elementFactory = realmTestingServer.GetRequiredService<IElementFactory>();
        var pickup = elementFactory.CreatePickup(Vector3.Zero, 1337);
        int elementEntered = 0;
        int elementLeft = 0;
        pickup.CollisionDetection.Entered += (that, e) =>
        {
            elementEntered++;
        };
        pickup.CollisionDetection.Left += (that, e) =>
        {
            elementLeft++;
        };
        #endregion

        #region Act
        pickup.CheckElementWithin(player);
        player.Position = new Vector3(100, 0, 0);
        pickup.CheckElementWithin(player);
        #endregion

        #region Assert
        elementEntered.Should().Be(1);
        elementLeft.Should().Be(1);
        #endregion
    }
}
