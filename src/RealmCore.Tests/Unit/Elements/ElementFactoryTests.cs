﻿namespace RealmCore.Tests.Unit.Elements;

public class ElementFactoryTests : RealmUnitTestingBase
{
    [Fact]
    public void ScopedElementFactoryShouldWork()
    {
        var server = CreateServer();
        var player = CreatePlayer();

        bool wasDestroyed = false;
        void handleDestroyed(Element element)
        {
            wasDestroyed = true;
        }

        var elementFactory = player.ElementFactory;
        var obj = elementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        obj.Destroyed += handleDestroyed;
        obj.Id.Value.Should().Be(30001);

        player.TriggerDisconnected(QuitReason.Quit);

        wasDestroyed.Should().BeTrue();
    }

    [Fact]
    public void InnerScopesForScopedElementFactoryShouldWork()
    {
        var server = CreateServer();
        var player = CreatePlayer();

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
    public void RootAndInnerScopesForScopedElementFactoryShouldWork()
    {
        var server = CreateServer();
        var player = CreatePlayer();

        var rootElementFactory = player.ElementFactory;
        var obj1 = rootElementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        var scope = rootElementFactory.CreateScope();
        var obj2 = scope.CreateObject(Location.Zero, (ObjectModel)1337);

        scope.CreatedElements.Should().BeEquivalentTo([obj2]);
        rootElementFactory.CreatedElements.Should().BeEquivalentTo([obj1, obj2]);

        player.TriggerDisconnected(QuitReason.Quit);

        rootElementFactory.CreatedElements.Should().BeEmpty();
    }

    [Fact]
    public void RootAndInnerScopesForScopedElementFactoryShouldWork2()
    {
        var server = CreateServer();
        var player = CreatePlayer();

        var rootElementFactory = player.ElementFactory;
        var obj1 = rootElementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        {
            using var scope = rootElementFactory.CreateScope();
            var obj2 = scope.CreateObject(Location.Zero, (ObjectModel)1337);

            scope.CreatedElements.Should().BeEquivalentTo([obj2]);
            rootElementFactory.CreatedElements.Should().BeEquivalentTo([obj1, obj2]);
        }
        rootElementFactory.CreatedElements.Should().BeEquivalentTo([obj1]);

        player.TriggerDisconnected(QuitReason.Quit);

        rootElementFactory.CreatedElements.Should().BeEmpty();
    }

    [Fact]
    public void ElementsShouldBeCreatedInTheSameDimensionAndInterior()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        player.Interior = 13;
        player.Dimension = 56;
        var rootElementFactory = player.ElementFactory;

        var obj = rootElementFactory.CreateObject(Location.Zero, (ObjectModel)1337);
        obj.Interior.Should().Be(13);
        obj.Dimension.Should().Be(56);
    }
}
