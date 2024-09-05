namespace RealmCore.Tests.Unit.Elements;

public class ElementFactoryTests : IClassFixture<RealmTestingServerHostingFixtureWithUniquePlayer>
{
    private readonly RealmTestingServerHostingFixtureWithUniquePlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;

    public ElementFactoryTests(RealmTestingServerHostingFixtureWithUniquePlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
    }

    [Fact]
    public async Task ScopedElementFactoryShouldWork()
    {
        var player = await _hosting.CreatePlayer();

        bool wasDestroyed = false;
        void handleDestroyed(Element element)
        {
            wasDestroyed = true;
        }

        var elementFactory = player.ElementFactory;
        var obj = elementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        obj.Destroyed += handleDestroyed;
        obj.Id.Value.Should().Be(30001);

        await _hosting.DisconnectPlayer(player);

        wasDestroyed.Should().BeTrue();
    }

    [Fact]
    public void InnerScopesForScopedElementFactoryShouldWork()
    {
        bool wasDestroyed = false;
        void handleDestroyed(Element element)
        {
            wasDestroyed = true;
        }

        {
            using var scope = _player.ElementFactory.CreateScope();
            var obj = scope.CreateObject(Location.Zero, (ObjectModel)1337);
            obj.Destroyed += handleDestroyed;
        }

        wasDestroyed.Should().BeTrue();
    }

    [Fact]
    public async Task RootAndInnerScopesForScopedElementFactoryShouldWork()
    {
        var player = await _hosting.CreatePlayer();

        var rootElementFactory = player.ElementFactory;
        var obj1 = rootElementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        var scope = rootElementFactory.CreateScope();
        var obj2 = scope.CreateObject(Location.Zero, (ObjectModel)1337);

        scope.CreatedElements.Should().BeEquivalentTo([obj2]);
        rootElementFactory.CreatedElements.Should().BeEquivalentTo([obj1, obj2]);

        await _hosting.DisconnectPlayer(player);

        rootElementFactory.CreatedElements.Should().BeEmpty();
    }

    [Fact]
    public async Task RootAndInnerScopesForScopedElementFactoryShouldWork2()
    {
        var player = await _hosting.CreatePlayer();

        var rootElementFactory = player.ElementFactory;
        var obj1 = rootElementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        {
            using var scope = rootElementFactory.CreateScope();
            var obj2 = scope.CreateObject(Location.Zero, (ObjectModel)1337);

            scope.CreatedElements.Should().BeEquivalentTo([obj2]);
            rootElementFactory.CreatedElements.Should().BeEquivalentTo([obj1, obj2]);
        }
        rootElementFactory.CreatedElements.Should().BeEquivalentTo([obj1]);

        await _hosting.DisconnectPlayer(player);

        rootElementFactory.CreatedElements.Should().BeEmpty();
    }

    [Fact]
    public void ElementsShouldBeCreatedInTheSameDimensionAndInterior()
    {
        _player.Interior = 13;
        _player.Dimension = 56;
        var rootElementFactory = _player.ElementFactory;

        var obj = rootElementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        obj.Interior.Should().Be(13);
        obj.Dimension.Should().Be(56);
    }
}
