namespace Realm.Domain.New;

[NoDefaultScriptAccess]
public sealed class VehicleElementComponent : Component
{
    private readonly Vehicle _vehicle;

    public Vehicle Vehicle => _vehicle;

    public VehicleElementComponent(Vehicle vehicle)
    {
        _vehicle = vehicle;
    }
}
