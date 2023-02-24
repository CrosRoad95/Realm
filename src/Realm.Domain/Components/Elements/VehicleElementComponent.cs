namespace Realm.Domain.Components.Elements;

public class VehicleElementComponent : ElementComponent
{
    protected readonly Vehicle _vehicle;

    internal Vehicle Vehicle => _vehicle;

    internal override Element Element => _vehicle;

    public string Name => _vehicle.Name;
    public ushort Model => _vehicle.Model;
    public bool IsEngineOn { get => _vehicle.IsEngineOn; set => _vehicle.IsEngineOn = value; }
    public bool IsLocked { get => _vehicle.IsLocked; set => _vehicle.IsLocked = value; }
    public float Health { get => _vehicle.Health; set => _vehicle.Health = value; }
    public bool IsDamageProof { get => _vehicle.IsDamageProof; set => _vehicle.IsDamageProof = value; }
    public bool AreDoorsDamageProof { get => _vehicle.AreDoorsDamageProof; set => _vehicle.AreDoorsDamageProof = value; }
    public float[] DoorRatios { get => _vehicle.DoorRatios; set => _vehicle.DoorRatios = value; }
    public Color PrimaryColor { get => _vehicle.Colors.Primary; set => _vehicle.Colors.Primary = value; }
    public Color SecondaryColor { get => _vehicle.Colors.Secondary; set => _vehicle.Colors.Secondary = value; }
    public Color Color3 { get => _vehicle.Colors.Color3; set => _vehicle.Colors.Color3 = value; }
    public Color Color4 { get => _vehicle.Colors.Color4; set => _vehicle.Colors.Color4 = value; }
    public byte PaintJob { get => _vehicle.PaintJob; set => _vehicle.PaintJob = value; }

    public void BlowUp()
    {
        _vehicle.BlowUp();
    }

    public void SetDoorState(VehicleDoor door, VehicleDoorState state, bool spawnFlyingComponent = false)
    {
        _vehicle.SetDoorState(door, state, spawnFlyingComponent);
    }

    public void SetWheelState(VehicleWheel wheel, VehicleWheelState state)
    {
        _vehicle.SetWheelState(wheel, state);
    }

    public void SetPanelState(VehiclePanel panel, VehiclePanelState state)
    {
        _vehicle.SetPanelState(panel, state);
    }

    public void SetLightState(VehicleLight light, VehicleLightState state)
    {
        _vehicle.SetLightState(light, state);
    }

    public void SetDoorOpenRatio(VehicleDoor door, float ratio, uint time = 0u)
    {
        _vehicle.SetDoorOpenRatio(door, ratio, time);
    }

    public void AddPassenger(byte seat, Entity pedEntity, bool warpsIn = true)
    {
        _vehicle.AddPassenger(seat, pedEntity.GetRequiredComponent<PedElementComponent>().Ped, warpsIn);
    }

    internal VehicleElementComponent(Vehicle vehicle)
    {
        _vehicle = vehicle;
    }
}
