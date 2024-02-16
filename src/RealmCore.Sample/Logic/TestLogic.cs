namespace RealmCore.Sample.Logic;

internal class TestLogic
{
    public TestLogic(IElementFactory elementFactory)
    {
        var marker = elementFactory.CreateMarker(new Location(335.50684f, -83.71094f, 1.4105641f), MarkerType.Cylinder, 1, Color.Red);
        marker.Size = 4;
        marker.CollisionDetection.AddRule<MustBeVehicleRule>();
    }
}
