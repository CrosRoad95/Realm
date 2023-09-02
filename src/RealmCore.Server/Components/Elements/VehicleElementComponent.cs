namespace RealmCore.Server.Components.Elements;

public class VehicleElementComponent : ElementComponent
{
    protected readonly Vehicle _vehicle;
    private readonly IEntityEngine _entityEngine;

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

    public string PlateText
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.PlateText;
        }
        set
        {
            ThrowIfDisposed();
            _vehicle.PlateText = value;
        }
    }


    public Dictionary<byte, Entity> Occupants
    {
        get
        {
            ThrowIfDisposed();
            return _vehicle.Occupants.ToDictionary(x => x.Key, x => _entityEngine.GetByElement(x.Value));
        }
    }

    public Entity? Driver
    {
        get
        {
            ThrowIfDisposed();

            if (_vehicle.Driver != null && _entityEngine.TryGetByElement(_vehicle.Driver, out var playerEntity))
                return playerEntity;
            return null;
        }
    }

    public event Action<VehicleElementComponent, VehiclePushedEventArgs>? Pushed;
    public event Action<VehicleElementComponent, VehicleLightStateChangedArgs>? LightStateChanged;
    public event Action<VehicleElementComponent, VehiclePanelStateChangedArgs>? PanelStateChanged;
    public event Action<VehicleElementComponent, VehicleWheelStateChangedArgs>? WheelStateChanged;
    public event Action<VehicleElementComponent, VehicleDoorStateChangedArgs>? DoorStateChanged;
    public event Action<VehicleElementComponent, float, float>? HealthChanged;
    public event Action<VehicleElementComponent>? Blown;
    public event Action<VehicleElementComponent, Entity>? PedEntered;
    public event Action<VehicleElementComponent, Entity>? PedLeft;
    public event Action<VehicleElementComponent>? Damaged;

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
        _vehicle.AddPassenger(seat, (Ped)pedEntity.GetElement(), warpsIn);
    }

    public void RemovePassenger(Entity pedEntity, bool warpsOut = true)
    {
        ThrowIfDisposed();
        _vehicle.RemovePassenger((Ped)pedEntity.GetElement(), warpsOut);
    }

    internal VehicleElementComponent(Vehicle vehicle, IEntityEngine entityEngine)
    {
        _vehicle = vehicle;
        _entityEngine = entityEngine;
    }

    protected override void Attach()
    {
        _vehicle.RespawnPosition = _vehicle.Position;
        _vehicle.RespawnRotation = _vehicle.Rotation;

        _vehicle.Pushed += HandlePushed;
        _vehicle.LightStateChanged += HandleLightStateChanged;
        _vehicle.PanelStateChanged += HandlePanelStateChanged;
        _vehicle.WheelStateChanged += HandleWheelStateChanged;
        _vehicle.DoorStateChanged += HandleDoorStateChanged;
        _vehicle.HealthChanged += HandleHealthChanged;
        _vehicle.Blown += HandleBlown;
        _vehicle.PedEntered += HandlePedEntered;
        _vehicle.PedLeft += HandlePedLeft;
        base.Attach();
    }

    private void HandlePedLeft(Element ped, VehicleLeftEventArgs e)
    {
        PedLeft?.Invoke(this, _entityEngine.GetByElement(ped));
    }

    private void HandlePedEntered(Element ped, VehicleEnteredEventsArgs e)
    {
        PedEntered?.Invoke(this, _entityEngine.GetByElement(ped));
    }

    private void HandleBlown(Element sender, VehicleBlownEventArgs e)
    {
        Blown?.Invoke(this);
    }

    private void HandleHealthChanged(Vehicle sender, ElementChangedEventArgs<Vehicle, float> args)
    {
        HealthChanged?.Invoke(this, args.OldValue, args.NewValue);
        if (args.IsSync)
            Damaged?.Invoke(this);
    }

    private void HandlePushed(Vehicle sender, VehiclePushedEventArgs e)
    {
        Pushed?.Invoke(this, e);
    }

    private void HandleLightStateChanged(Element sender, VehicleLightStateChangedArgs args)
    {
        LightStateChanged?.Invoke(this, args);
        if(args.State != VehicleLightState.Intact)
            Damaged?.Invoke(this);
    }

    private void HandlePanelStateChanged(Element sender, VehiclePanelStateChangedArgs args)
    {
        PanelStateChanged?.Invoke(this, args);
        if (args.State != VehiclePanelState.Undamaged)
            Damaged?.Invoke(this);
    }

    private void HandleWheelStateChanged(Element sender, VehicleWheelStateChangedArgs args)
    {
        WheelStateChanged?.Invoke(this, args);
        if (args.State != VehicleWheelState.Inflated)
            Damaged?.Invoke(this);
    }

    private void HandleDoorStateChanged(Element sender, VehicleDoorStateChangedArgs args)
    {
        DoorStateChanged?.Invoke(this, args);
        if (args.State != VehicleDoorState.ShutIntact)
            Damaged?.Invoke(this);
    }
}
