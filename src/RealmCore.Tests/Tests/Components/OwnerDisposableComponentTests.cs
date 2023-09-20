namespace RealmCore.Tests.Tests.Components;

public class OwnerDisposableComponentTests
{
    [Fact]
    public void OwnerDisposableComponentShouldRemoveAppropriateEntity()
    {
        bool disposed = false;
        var entity1 = new Entity();
        var entity2 = new Entity();

        entity1.Disposed += _ =>
        {
            disposed = true;
        };

        entity1.AddComponent(new OwnerDisposableComponent(entity2));
        entity2.Dispose();

        disposed.Should().BeTrue();
    }

    [Fact]
    public void OwnerDisposableComponentCanNotOwnItself()
    {
        var entity = new Entity();

        var act = () => entity.AddComponent(new OwnerDisposableComponent(entity));

        act.Should().Throw<ArgumentException>();
        entity.ComponentsCount.Should().Be(0);
    }

    [Fact]
    public void RemovingOwnerDisposableComponentShouldUnregisterEvents()
    {
        bool disposed = false;
        var entity1 = new Entity();
        var entity2 = new Entity();

        entity1.Disposed += _ =>
        {
            disposed = true;
        };

        entity1.AddComponent(new OwnerDisposableComponent(entity2));
        entity1.DestroyComponent<OwnerDisposableComponent>();
        entity2.Dispose();

        disposed.Should().BeFalse();
    }
}
