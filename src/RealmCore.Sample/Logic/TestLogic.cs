namespace RealmCore.Sample.Logic;

public class TestComponent : Component { }

internal class TestLogic : ComponentLogic<PlayerTagComponent>
{
    public TestLogic(IEntityEngine entityEngine) : base(entityEngine)
    {
    }

    protected override void ComponentAdded(PlayerTagComponent component)
    {
        component.Entity.AddComponent<TestComponent>();
    }
}
