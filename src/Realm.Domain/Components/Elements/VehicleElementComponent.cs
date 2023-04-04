namespace Realm.Domain.Components.Elements;

public class VehicleElementComponent : ElementComponent
{
    [Inject]
    private IECS _ecs { get; set; } = default!;

    protected readonly Vehicle _vehicle;

    internal Vehicle Vehicle => _vehicle;

    internal override Element Element => _vehicle;

    public string Name
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.Name;
        }
    }

    public ushort Model
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.Model;
        }
    }

    public bool IsEngineOn
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.IsEngineOn;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.IsEngineOn = value;
        }
    }

    public bool IsLocked
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.IsLocked;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.IsLocked = value;
        }
    }

    public float Health
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.Health;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.Health = value;
        }
    }

    public bool IsDamageProof
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.IsDamageProof;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.IsDamageProof = value;
        }
    }

    public bool AreDoorsDamageProof
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.AreDoorsDamageProof;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.AreDoorsDamageProof = value;
        }
    }

    public float[] DoorRatios
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.DoorRatios;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.DoorRatios = value;
        }
    }

    public Color PrimaryColor
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.Colors.Primary;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.Colors.Primary = value;
        }
    }

    public Color SecondaryColor
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.Colors.Secondary;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.Colors.Secondary = value;
        }
    }

    public Color Color3
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.Colors.Color3;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.Colors.Color3 = value;
        }
    }

    public Color Color4
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.Colors.Color4;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.Colors.Color4 = value;
        }
    }

    public byte PaintJob
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.PaintJob;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.PaintJob = value;
        }
    }

    public Dictionary<byte, Entity> Occupants
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.Occupants.ToDictionary(x => x.Key, x => _ecs.GetByElement(x.Value));
        }
    }

    public event Action<VehicleElementComponent, VehiclePushedEventArgs>? Pushed;
    public event Action<VehicleElementComponent, VehicleLightStateChangedArgs>? LightStateChanged;
    public event Action<VehicleElementComponent, VehiclePanelStateChangedArgs>? PanelStateChanged;
    public event Action<VehicleElementComponent, VehicleWheelStateChangedArgs>? WheelStateChanged;
    public event Action<VehicleElementComponent, VehicleDoorStateChangedArgs>? DoorStateChanged;
    public event Action<VehicleElementComponent, float, float>? HealthChanged;
    public event Action<VehicleElementComponent>? Blown;

    public void BlowUp()
    {
        ThrowIfDisposed();
        _vehicle.BlowUp();
    }

    public void Respawn()
    {
        ThrowIfDisposed();
        _vehicle.Respawn();
    }

    public void SetDoorState(VehicleDoor door, VehicleDoorState state, bool spawnFlyingComponent = false)
    {
        ThrowIfDisposed();
        _vehicle.SetDoorState(door, state, spawnFlyingComponent);
    }

    public void SetWheelState(VehicleWheel wheel, VehicleWheelState state)
    {
        ThrowIfDisposed();
        _vehicle.SetWheelState(wheel, state);
    }

    public void SetPanelState(VehiclePanel panel, VehiclePanelState state)
    {
        ThrowIfDisposed();
        _vehicle.SetPanelState(panel, state);
    }

    public void SetLightState(VehicleLight light, VehicleLightState state)
    {
        ThrowIfDisposed();
        _vehicle.SetLightState(light, state);
    }

    public void SetDoorOpenRatio(VehicleDoor door, float ratio, uint time = 0u)
    {
        ThrowIfDisposed();
        _vehicle.SetDoorOpenRatio(door, ratio, time);
    }

    public void AddPassenger(byte seat, Entity pedEntity, bool warpsIn = true)
    {
        ThrowIfDisposed();
        _vehicle.AddPassenger(seat, (Ped)pedEntity.Element, warpsIn);
    }

    public void RemovePasssenger(Entity pedEntity, bool warpsOut = true)
    {
        ThrowIfDisposed();
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
        base.Load();
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
