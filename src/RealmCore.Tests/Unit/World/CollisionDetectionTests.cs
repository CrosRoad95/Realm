namespace RealmCore.Tests.Unit.World;

public class CollisionDetectionTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task CollisionSphereDetection(bool usePlayerElementFactory)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        IElementFactory elementFactory;
        if (usePlayerElementFactory)
        {
            elementFactory = player.ElementFactory;
        }
        else
        {
            elementFactory = hosting.GetRequiredService<IElementFactory>();
        }

        var position = new Vector3(100, 0, 0);
        var collisionSphere = elementFactory.CreateCollisionSphere(position, 10);
        var realmWorldObject = elementFactory.CreateObject(Location.Zero, ObjectModel.Vegtree3);
        int elementEntered = 0;
        int elementLeft = 0;
        collisionSphere.ElementEntered += (that, e) =>
        {
            elementEntered++;
        };
        collisionSphere.ElementLeft += (that, e) =>
        {
            elementLeft++;
        };

        player.Position = position;
        realmWorldObject.Position = position;
        player.Position = new Vector3(0, 0, 0);
        realmWorldObject.Position = new Vector3(0, 0, 0);

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
    public async Task MarkerDetection(bool usePlayerElementFactory)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        IElementFactory elementFactory;
        if (usePlayerElementFactory)
        {
            elementFactory = player.ElementFactory;
        }
        else
        {
            elementFactory = hosting.GetRequiredService<IElementFactory>();
        }

        var position = new Vector3(100, 0, 0);
        var marker = elementFactory.CreateMarker(new Location(position), MarkerType.Cylinder, 1, Color.Red);
        var realmWorldObject = elementFactory.CreateObject(Location.Zero, ObjectModel.Vegtree3);

        int elementEntered = 0;
        int elementLeft = 0;
        marker.CollisionShape.ElementEntered += (that, e) =>
        {
            elementEntered++;
        };
        marker.CollisionShape.ElementLeft += (that, e) =>
        {
            elementLeft++;
        };

        player.Position = position;
        realmWorldObject.Position = position;
        player.Position = new Vector3(0, 0, 0);
        realmWorldObject.Position = new Vector3(0, 0, 0);

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
    public async Task PickupDetection(bool usePlayerElementFactory)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        IElementFactory elementFactory;
        if (usePlayerElementFactory)
        {
            elementFactory = player.ElementFactory;
        }
        else
        {
            elementFactory = hosting.GetRequiredService<IElementFactory>();
        }

        var position = new Vector3(100, 0, 0);
        var realmWorldObject = elementFactory.CreateObject(Location.Zero, ObjectModel.Vegtree3);
        var pickup = elementFactory.CreatePickup(Location.Zero, 1337);
        int elementEntered = 0;
        pickup.Used += (that, e) =>
        {
            elementEntered++;
        };

        player.Position = position;
        realmWorldObject.Position = position;
        player.Position = new Vector3(0, 0, 0);
        realmWorldObject.Position = new Vector3(0, 0, 0);

        using var _ = new AssertionScope();
        elementEntered.Should().Be(1);
    }
}
