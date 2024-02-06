namespace RealmCore.Sample.Logic;

public class TestComponent : Component { }

internal class TestLogic
{
    public TestLogic(IElementFactory elementFactory)
    {
        var marker = elementFactory.CreateMarker(new Vector3(335.50684f, -83.71094f, 1.4105641f), MarkerType.Cylinder, 1, Color.Red);
        marker.Size = 4;
        marker.CollisionDetection.AddRule<MustBeVehicleRule>();
    }
}
