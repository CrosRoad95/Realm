namespace RealmCore.Tests.Unit.Elements;

public class ElementFactoryTests
{
    [Fact]
    public async Task ScopedElementFactoryShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        bool wasDestroyed = false;
        void handleDestroyed(Element element)
        {
            wasDestroyed = true;
        }

        var elementFactory = player.ElementFactory;
        var obj = elementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        obj.Destroyed += handleDestroyed;
        obj.Id.Value.Should().Be(30001);

        await hosting.DisconnectPlayer(player);

        wasDestroyed.Should().BeTrue();
    }

    [Fact]
    public async Task InnerScopesForScopedElementFactoryShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        bool wasDestroyed = false;
        void handleDestroyed(Element element)
        {
            wasDestroyed = true;
        }

        {
            using var scope = player.ElementFactory.CreateScope();
            var obj = scope.CreateObject(Location.Zero, (ObjectModel)1337);
            obj.Destroyed += handleDestroyed;
        }

        wasDestroyed.Should().BeTrue();
    }

    [Fact]
    public async Task RootAndInnerScopesForScopedElementFactoryShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var rootElementFactory = player.ElementFactory;
        var obj1 = rootElementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        var scope = rootElementFactory.CreateScope();
        var obj2 = scope.CreateObject(Location.Zero, (ObjectModel)1337);

        scope.CreatedElements.Should().BeEquivalentTo([obj2]);
        rootElementFactory.CreatedElements.Should().BeEquivalentTo([obj1, obj2]);

        await hosting.DisconnectPlayer(player);

        rootElementFactory.CreatedElements.Should().BeEmpty();
    }

    [Fact]
    public async Task RootAndInnerScopesForScopedElementFactoryShouldWork2()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var rootElementFactory = player.ElementFactory;
        var obj1 = rootElementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        {
            using var scope = rootElementFactory.CreateScope();
            var obj2 = scope.CreateObject(Location.Zero, (ObjectModel)1337);

            scope.CreatedElements.Should().BeEquivalentTo([obj2]);
            rootElementFactory.CreatedElements.Should().BeEquivalentTo([obj1, obj2]);
        }
        rootElementFactory.CreatedElements.Should().BeEquivalentTo([obj1]);

        await hosting.DisconnectPlayer(player);

        rootElementFactory.CreatedElements.Should().BeEmpty();
    }

    [Fact]
    public async Task ElementsShouldBeCreatedInTheSameDimensionAndInterior()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        player.Interior = 13;
        player.Dimension = 56;
        var rootElementFactory = player.ElementFactory;

        var obj = rootElementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        obj.Interior.Should().Be(13);
        obj.Dimension.Should().Be(56);
    }
}
