namespace RealmCore.Tests.Tests.Components;

public class OwnerComponentTests
{
    [Fact]
    public void OwnerComponentTestsShouldWork()
    {
        var entity1 = new Entity();
        var entity2 = new Entity();

        entity1.AddComponent(new OwnerComponent(entity2));
        entity2.Dispose();

        entity1.Components.Should().BeEmpty();
    }
}
