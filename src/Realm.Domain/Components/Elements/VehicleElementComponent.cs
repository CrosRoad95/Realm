namespace Realm.Domain.Components.Elements;

public class VehicleElementComponent : ElementComponent
{
    protected readonly Vehicle _vehicle;

    internal Vehicle Vehicle => _vehicle;

    internal override Element Element => _vehicle;

    public string Name => _vehicle.Name;
    public ushort Model => _vehicle.Model;
    public bool IsEngineOn { get => _vehicle.IsEngineOn; set => _vehicle.IsEngineOn = value; }
    public bool IsFrozen { get => _vehicle.IsFrozen; set => _vehicle.IsFrozen = value; }
    public bool IsLocked { get => _vehicle.IsLocked; set => _vehicle.IsLocked = value; }

    internal VehicleElementComponent(Vehicle vehicle)
    {
        _vehicle = vehicle;
    }
}
