namespace Realm.Domain.Components.Elements;

public class VehicleElementComponent : ElementComponent
{
    protected readonly Vehicle _vehicle;

    internal Vehicle Vehicle => _vehicle;

    internal override Element Element => _vehicle;

    public ushort Model => _vehicle.Model;
    public bool IsEngineOn => _vehicle.IsEngineOn;
    public bool IsFrozen => _vehicle.IsFrozen;

    internal VehicleElementComponent(Vehicle vehicle)
    {
        _vehicle = vehicle;
    }
}
