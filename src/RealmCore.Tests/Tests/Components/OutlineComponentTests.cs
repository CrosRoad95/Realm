using RealmCore.Tests.Classes.Components;

namespace RealmCore.Tests.Tests.Components;

public class OutlineComponentTests
{
    [Fact]
    public void SimpleOutlineComponentShouldWork()
    {
        var entity = new Entity();
        entity.AddComponent<TestElementComponent>();
        entity.AddComponent(new OutlineComponent(Color.Red));
    }

    [Fact]
    public void OutlineComponentShouldWork()
    {
        var realmTestingServer = new RealmTestingServer(null);
        var elementFactory = realmTestingServer.GetRequiredService<IElementFactory>();
        var entity = elementFactory.CreateObject();

        entity.AddComponent<TestElementComponent>();
        entity.AddComponent(new OutlineComponent(Color.Red));
    }
}
