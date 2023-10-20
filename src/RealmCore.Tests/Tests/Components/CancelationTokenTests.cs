using RealmCore.Tests.Classes.Components;

namespace RealmCore.Tests.Tests.Components;

public class CancelationTokenTests
{
    [Fact]
    public void TokenShouldBeCanceledWhenComponentDetaches()
    {
        var entity = new Entity();
        var component = entity.AddComponent<TestComponent>();
        var ct = Component.CreateCancelationToken(component);
        entity.DestroyComponent(component);
        ct.IsCancellationRequested.Should().BeTrue();
    }
}
