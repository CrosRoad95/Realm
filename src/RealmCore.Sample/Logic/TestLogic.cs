using System.Drawing;

namespace RealmCore.Sample.Logic;

public class TestComponent : Component { }

internal class TestLogic : ComponentLogic<PlayerTagComponent>
{
    public TestLogic(IEntityEngine entityEngine, IEntityFactory entityFactory) : base(entityEngine)
    {
        var marker = entityFactory.CreateMarker(MarkerType.Cylinder, new Vector3(335.50684f, -83.71094f, 1.4105641f), Color.Red);
        var markerElementComponent = marker.GetRequiredComponent<MarkerElementComponent>();
        markerElementComponent.Size = 4;
        markerElementComponent.EntityEntered = HandleEntityEntered;
        markerElementComponent.AddRule<MustBeVehicleRule>();
    }

    private void HandleEntityEntered(MarkerElementComponent markerElementComponent, Entity enteredMarkerEntity, Entity entity)
    {
        ;
    }

    protected override void ComponentAdded(PlayerTagComponent component)
    {
        component.Entity.AddComponent<TestComponent>();
    }
}
