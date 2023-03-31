namespace Realm.Domain.Components.Elements;

public class VehicleElementComponent : ElementComponent
{
    [Inject]
    private IECS _ecs { get; set; }

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
    public Dictionary<byte, Entity> Occupants => _vehicle.Occupants.ToDictionary(x => x.Key, x => _ecs.GetByElement(x.Value));
    
    public event Action<VehicleElementComponent, VehiclePushedEventArgs> Pushed;
    public event Action<VehicleElementComponent, VehicleLightStateChangedArgs> LightStateChanged;
    public event Action<VehicleElementComponent, VehiclePanelStateChangedArgs> PanelStateChanged;
    public event Action<VehicleElementComponent, VehicleWheelStateChangedArgs> WheelStateChanged;
    public event Action<VehicleElementComponent, VehicleDoorStateChangedArgs> DoorStateChanged;
    public event Action<VehicleElementComponent, float, float> HealthChanged;
    public event Action<VehicleElementComponent> Blown;

    public void BlowUp()
    {
        _vehicle.BlowUp();
    }
    
    public void Respawn()
    {
        _vehicle.Respawn();
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
        _vehicle.AddPassenger(seat, (Ped)pedEntity.Element, warpsIn);
    }
    
    public void RemovePasssenger(Entity pedEntity, bool warpsOut = true)
    {
        _vehicle.RemovePassenger((Ped)pedEntity.Element, warpsOut);
    }

    internal VehicleElementComponent(Vehicle vehicle)
    {
        _vehicle = vehicle;
    }

    protected override void Load()
    {
        _vehicle.Pushed += HandlePushed;
        _vehicle.LightStateChanged += HandleLightStateChanged;
        _vehicle.PanelStateChanged += HandlePanelStateChanged;
        _vehicle.WheelStateChanged += HandleWheelStateChanged;
        _vehicle.DoorStateChanged += HandleDoorStateChanged;
        _vehicle.HealthChanged += HandleHealthChanged;
        _vehicle.Blown += HandleBlown;
    }

    private void HandleBlown(Element sender)
    {
        Blown?.Invoke(this);
    }

    private void HandleHealthChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, float> args)
    {
        HealthChanged?.Invoke(this, args.OldValue, args.NewValue);
    }

    private void HandlePushed(Vehicle sender, VehiclePushedEventArgs e)
    {
        Pushed?.Invoke(this, e);
    }

    private void HandleLightStateChanged(Element sender, VehicleLightStateChangedArgs e)
    {
        LightStateChanged?.Invoke(this, e);
    }

    private void HandlePanelStateChanged(Element sender, VehiclePanelStateChangedArgs e)
    {
        PanelStateChanged?.Invoke(this, e);
    }

    private void HandleWheelStateChanged(Element sender, VehicleWheelStateChangedArgs e)
    {
        WheelStateChanged?.Invoke(this, e);
    }

    private void HandleDoorStateChanged(Element sender, VehicleDoorStateChangedArgs e)
    {
        DoorStateChanged?.Invoke(this, e);
    }
}
