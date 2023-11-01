namespace RealmCore.Server.Components.Vehicles;

public class PrivateVehicleComponent : Component
{
    private readonly VehicleData _vehicleData;
    private readonly IDateTimeProvider _dateTimeProvider;

    public int Id => _vehicleData.Id;
    public byte Kind
    {
        get => _vehicleData.Kind;
        internal set
        {
            _vehicleData.Kind = value;
        }
    }

    public DateTime? LastUsed
    {
        get => _vehicleData.LastUsed;
        private set
        {
            _vehicleData.LastUsed = value;
        }
    }

    private readonly VehicleAccess _access;
    public VehicleAccess Access => _access;

    internal PrivateVehicleComponent(VehicleData vehicleData, IDateTimeProvider dateTimeProvider)
    {
        _vehicleData = vehicleData;
        _dateTimeProvider = dateTimeProvider;
        _access = new VehicleAccess(_vehicleData.UserAccesses);
    }

    public void UpdateLastUsed()
    {
        LastUsed = _dateTimeProvider.Now;
    }

    public void Attach()
    {
        var vehicle = (RealmVehicle)Element;
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
    }
}
