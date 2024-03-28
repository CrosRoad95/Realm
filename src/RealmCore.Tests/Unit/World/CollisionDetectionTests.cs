namespace RealmCore.Tests.Unit.World;

public class CollisionDetectionTests : RealmUnitTestingBase
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void CollisionSphereDetection(bool usePlayerElementFactory)
    {
        var server = CreateServer();
        var player = CreatePlayer();

        IElementFactory elementFactory;
        if (usePlayerElementFactory)
        {
            elementFactory = player.ElementFactory;
        }
        else
        {
            elementFactory = server.GetRequiredService<IElementFactory>();
        }

        var realmWorldObject = elementFactory.CreateObject(Location.Zero, ObjectModel.Vegtree3);
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

        collisionSphere.CheckElementWithin(player);
        player.Position = new Vector3(100, 0, 0);
        collisionSphere.CheckElementWithin(player);

        collisionSphere.CheckElementWithin(realmWorldObject);
        realmWorldObject.Position = new Vector3(100, 0, 0);
        collisionSphere.CheckElementWithin(realmWorldObject);

        using var _ = new AssertionScope();

        if (usePlayerElementFactory)
        {
            elementEntered.Should().Be(1);
            elementLeft.Should().Be(1);
        }
        else
        {
            elementEntered.Should().Be(2);
            elementLeft.Should().Be(2);
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void MarkerDetection(bool usePlayerElementFactory)
    {
        var server = CreateServer();
        var player = CreatePlayer();

        IElementFactory elementFactory;
        if (usePlayerElementFactory)
        {
            elementFactory = player.ElementFactory;
        }
        else
        {
            elementFactory = server.GetRequiredService<IElementFactory>();
        }

        var realmWorldObject = elementFactory.CreateObject(Location.Zero, ObjectModel.Vegtree3);
        var marker = elementFactory.CreateMarker(Location.Zero, MarkerType.Cylinder, 1, Color.Red);
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

        marker.CheckElementWithin(player);
        player.Position = new Vector3(100, 0, 0);
        marker.CheckElementWithin(player);

        marker.CheckElementWithin(realmWorldObject);
        realmWorldObject.Position = new Vector3(100, 0, 0);
        marker.CheckElementWithin(realmWorldObject);

        using var _ = new AssertionScope();
        elementEntered.Should().Be(2);
        elementLeft.Should().Be(2);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void PickupDetection(bool usePlayerElementFactory)
    {
        var server = CreateServer();
        var player = CreatePlayer();

        IElementFactory elementFactory;
        if (usePlayerElementFactory)
        {
            elementFactory = player.ElementFactory;
        }
        else
        {
            elementFactory = server.GetRequiredService<IElementFactory>();
        }

        var realmWorldObject = elementFactory.CreateObject(Location.Zero, ObjectModel.Vegtree3);
        var pickup = elementFactory.CreatePickup(Location.Zero, 1337);
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

        pickup.CheckElementWithin(player);
        player.Position = new Vector3(100, 0, 0);
        pickup.CheckElementWithin(player);

        pickup.CheckElementWithin(realmWorldObject);
        realmWorldObject.Position = new Vector3(100, 0, 0);
        pickup.CheckElementWithin(realmWorldObject);

        using var _ = new AssertionScope();
        elementEntered.Should().Be(2);
        elementLeft.Should().Be(2);
    }
}
