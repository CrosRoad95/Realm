namespace RealmCore.Tests.Unit.Elements;

public class ElementBagTests
{
    [Fact]
    public void OnlyOneElementOfGivenTypeCanBeAddedToBag()
    {
        var bag = new ElementBag();
        var element = new Element();

        bag.TryAdd(element).Should().BeTrue();
        bag.TryAdd(element).Should().BeFalse();

        bag.Should().BeEquivalentTo([element]);
    }

    [Fact]
    public void DestroyedElementShouldBeRemovedFromBag()
    {
        var bag = new ElementBag();
        var element = new Element();

        bag.TryAdd(element);
        element.Destroy();

        bag.Should().BeEmpty();
    }

    [Fact]
    public void ElementCanBeRemovedFromBag()
    {
        var bag = new ElementBag();
        var element = new Element();

        bag.TryAdd(element);
        bag.TryRemove(element).Should().BeTrue();
        bag.TryRemove(element).Should().BeFalse();

        bag.Should().BeEmpty();
    }

    [Fact]
    public void BagCanBeDestroyed()
    {
        var element = new Element();

        {
            using var bag = new ElementBag();

            bag.TryAdd(element);
        }

        element.Destroy();
    }
}
