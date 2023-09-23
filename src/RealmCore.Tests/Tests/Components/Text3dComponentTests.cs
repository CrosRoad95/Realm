using RealmCore.Server.Components.World;

namespace RealmCore.Tests.Tests.Components;

public class Text3dComponentTests
{
    [Fact]
    public void Text3dComponentTest()
    {
        var entity = new Entity();
        entity.AddComponent(new Text3dComponent("Text", Vector3.Zero));
    }
}
