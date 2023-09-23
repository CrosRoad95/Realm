using RealmCore.Tests.Classes.Components;

namespace RealmCore.Tests.Tests.Components;

public class OutlineComponentTests
{
    [Fact]
    public void OutlineComponentShouldWork()
    {
        var entity = new Entity();
        entity.AddComponent<Transform>();
        entity.AddComponent<TestElementComponent>();
        entity.AddComponent(new OutlineComponent(Color.Red));
    }
}
