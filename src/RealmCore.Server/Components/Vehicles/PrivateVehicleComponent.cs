using VehiclePlayerAccess = RealmCore.Server.Concepts.VehiclePlayerAccess;

namespace RealmCore.Server.Components.Vehicles;

public class PrivateVehicleComponent : Component
{
    private readonly VehicleData _vehicleData;

    public int Id => _vehicleData.Id;

    private readonly object _lock = new();
    private List<VehiclePlayerAccess> _vehiclePlayerAccesses = new();
    public IReadOnlyList<VehiclePlayerAccess> PlayerAccesses
    {
        get
        {
            ThrowIfDisposed();
            lock (_lock)
                return new List<VehiclePlayerAccess>(_vehiclePlayerAccesses);
        }
    }

    public IReadOnlyList<VehiclePlayerAccess> Owners
    {
        get
        {
            ThrowIfDisposed();
            lock (_lock)
                return new List<VehiclePlayerAccess>(_vehiclePlayerAccesses.Where(x => x.AccessType == 0));
        }
    }

    internal PrivateVehicleComponent(VehicleData vehicleData)
    {
        _vehicleData = vehicleData;
    }

    protected override void Load()
    {
        var vehicleElementComponent = Entity.GetRequiredComponent<VehicleElementComponent>();
        var vehicle = vehicleElementComponent.Vehicle;
        vehicle.Colors.Primary = _vehicleData.Color.Color1;
        vehicle.Colors.Secondary = _vehicleData.Color.Color2;
        vehicle.Colors.Color3 = _vehicleData.Color.Color3;
        vehicle.Colors.Color4 = _vehicleData.Color.Color4;
        vehicle.PaintJob = (byte)_vehicleData.Paintjob;
        vehicle.PlateText = _vehicleData.Platetext;
        vehicle.Variants = new VehicleVariants
        {
            Variant1 = _vehicleData.Variant.Variant1,
            Variant2 = _vehicleData.Variant.Variant2,
        };
        vehicle.Damage.Lights[0] = (byte)_vehicleData.DamageState.FrontLeftLight;
        vehicle.Damage.Lights[1] = (byte)_vehicleData.DamageState.FrontRightLight;
        vehicle.Damage.Lights[2] = (byte)_vehicleData.DamageState.RearLeftLight;
        vehicle.Damage.Lights[3] = (byte)_vehicleData.DamageState.RearRightLight;
        vehicle.Damage.Panels[0] = (byte)_vehicleData.DamageState.FrontLeftPanel;
        vehicle.Damage.Panels[1] = (byte)_vehicleData.DamageState.FrontRightPanel;
        vehicle.Damage.Panels[2] = (byte)_vehicleData.DamageState.RearLeftPanel;
        vehicle.Damage.Panels[3] = (byte)_vehicleData.DamageState.RearRightPanel;
        vehicle.Damage.Panels[4] = (byte)_vehicleData.DamageState.Windscreen;
        vehicle.Damage.Panels[5] = (byte)_vehicleData.DamageState.FrontBumper;
        vehicle.Damage.Panels[6] = (byte)_vehicleData.DamageState.RearBumper;
        vehicle.Damage.Doors[0] = (byte)_vehicleData.DamageState.Hood;
        vehicle.Damage.Doors[1] = (byte)_vehicleData.DamageState.Trunk;
        vehicle.Damage.Doors[2] = (byte)_vehicleData.DamageState.FrontLeftDoor;
        vehicle.Damage.Doors[3] = (byte)_vehicleData.DamageState.FrontRightDoor;
        vehicle.Damage.Doors[4] = (byte)_vehicleData.DamageState.RearLeftDoor;
        vehicle.Damage.Doors[5] = (byte)_vehicleData.DamageState.RearRightDoor;
        vehicle.DoorRatios = new float[6] { _vehicleData.DoorOpenRatio.Hood, _vehicleData.DoorOpenRatio.Trunk, _vehicleData.DoorOpenRatio.FrontLeft,
            _vehicleData.DoorOpenRatio.FrontRight, _vehicleData.DoorOpenRatio.RearLeft, _vehicleData.DoorOpenRatio.RearRight };
        vehicle.SetWheelState(VehicleWheel.FrontLeft, (VehicleWheelState)_vehicleData.WheelStatus.FrontLeft);
        vehicle.SetWheelState(VehicleWheel.FrontRight, (VehicleWheelState)_vehicleData.WheelStatus.FrontRight);
        vehicle.SetWheelState(VehicleWheel.RearLeft, (VehicleWheelState)_vehicleData.WheelStatus.RearLeft);
        vehicle.SetWheelState(VehicleWheel.RearRight, (VehicleWheelState)_vehicleData.WheelStatus.RearRight);
        vehicle.IsEngineOn = _vehicleData.EngineState;
        vehicle.IsLandingGearDown = _vehicleData.LandingGearDown;
        vehicle.OverrideLights = (VehicleOverrideLights)_vehicleData.OverrideLights;
        vehicle.IsSirenActive = _vehicleData.SirensState;
        vehicle.IsLocked = _vehicleData.Locked;
        vehicle.IsTaxiLightOn = _vehicleData.TaxiLightState;
        vehicle.Health = _vehicleData.Health;
        vehicle.IsFrozen = _vehicleData.IsFrozen;
        _vehiclePlayerAccesses = _vehicleData.UserAccesses.Select(x => new VehiclePlayerAccess
        {
            Id = x.Id,
            UserId = x.User.Id,
            AccessType = x.AccessType,
            CustomValue = x.CustomValue
        }).ToList();
    }

    public bool TryGetAccess(Entity entity, out VehiclePlayerAccess vehicleAccess)
    {
        ThrowIfDisposed();

        if (entity.Tag != EntityTag.Player)
            throw new InvalidOperationException();

        var userId = entity.GetRequiredComponent<UserComponent>().Id;
        lock (_lock)
        {
            var index = _vehiclePlayerAccesses.FindIndex(x => x.UserId == userId);
            if (index >= 0)
            {
                vehicleAccess = _vehiclePlayerAccesses[index];
                return true;
            }
        }
        vehicleAccess = default;
        return false;
    }

    public bool HasAccess(Entity entity)
    {
        if(TryGetAccess(entity, out VehiclePlayerAccess vehiclePlayerAccess))
        {
            return true;
        }
        return false;
    }

    public VehiclePlayerAccess AddAccess(Entity entity, byte accessType, string? customValue = null)
    {
        ThrowIfDisposed();

        if (entity.Tag != EntityTag.Player)
            throw new InvalidOperationException();

        if (TryGetAccess(entity, out var _))
            throw new EntityAccessDefinedException();

        lock (_lock)
        {
            _vehiclePlayerAccesses.Add(new VehiclePlayerAccess
            {
                UserId = entity.GetRequiredComponent<UserComponent>().Id,
                AccessType = accessType,
                CustomValue = customValue
            });
            return _vehiclePlayerAccesses.Last();
        }
    }

    public VehiclePlayerAccess AddAsOwner(Entity entity, string? customValue = null)
    {
        ThrowIfDisposed();

        return AddAccess(entity, 0, customValue);
    }
}
