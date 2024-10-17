namespace RealmCore.Server.Modules.Vehicles.Persistence;

public interface IUsesVehiclePersistentData
{
    void Loaded(VehicleData vehicleData, bool preserveData = false);
    void Unloaded();
    event Action? VersionIncreased;
}

public sealed class VehiclePersistenceFeature : IVehicleFeature
{
    private readonly object _lock = new();
    private VehicleData? _vehicleData;
    private readonly IDateTimeProvider _dateTimeProvider;
    private int _version = 0;
    public VehicleData VehicleData => _vehicleData ?? throw new Exception("Vehicle not loaded");

    public bool IsLoaded => _vehicleData != null;
    public int Id => VehicleData.Id;
    public byte Kind
    {
        get => VehicleData.Kind;
        set => VehicleData.Kind = value;
    }
    
    public int Speedometer
    {
        get => VehicleData.Speedometer;
        set => VehicleData.Speedometer = value;
    }

    public DateTime? LastUsed
    {
        get => VehicleData.LastUsed;
        set => VehicleData.LastUsed = value;
    }

    public event Action<VehiclePersistenceFeature, RealmVehicle>? Loaded;

    public RealmVehicle Vehicle { get; init; }

    public VehiclePersistenceFeature(VehicleContext vehicleContext, IDateTimeProvider dateTimeProvider)
    {
        Vehicle = vehicleContext.Vehicle;
        _dateTimeProvider = dateTimeProvider;
    }

    public void Load(VehicleData vehicleData, bool preserveData = false)
    {
        lock (_lock)
        {
            if (_vehicleData != null)
                throw new VehicleAlreadyLoadedException();

            _vehicleData = vehicleData;
            Vehicle.Model = vehicleData.Model;
            if (!preserveData)
            {
                Vehicle.Position = vehicleData.TransformAndMotion.Position;
                Vehicle.Rotation = vehicleData.TransformAndMotion.Rotation;
                Vehicle.Interior = vehicleData.TransformAndMotion.Interior;
                Vehicle.Dimension = vehicleData.TransformAndMotion.Dimension;

                Vehicle.Colors.Primary = vehicleData.Color.Color1;
                Vehicle.Colors.Secondary = vehicleData.Color.Color2;
                Vehicle.Colors.Color3 = vehicleData.Color.Color3;
                Vehicle.Colors.Color4 = vehicleData.Color.Color4;
                Vehicle.PaintJob = (byte)vehicleData.Paintjob;
                Vehicle.PlateText = vehicleData.Platetext;
                Vehicle.Variants = new VehicleVariants
                {
                    Variant1 = vehicleData.Variant.Variant1,
                    Variant2 = vehicleData.Variant.Variant2,
                };
                Vehicle.Damage.Lights[0] = (byte)vehicleData.DamageState.FrontLeftLight;
                Vehicle.Damage.Lights[1] = (byte)vehicleData.DamageState.FrontRightLight;
                Vehicle.Damage.Lights[2] = (byte)vehicleData.DamageState.RearLeftLight;
                Vehicle.Damage.Lights[3] = (byte)vehicleData.DamageState.RearRightLight;
                Vehicle.Damage.Panels[0] = (byte)vehicleData.DamageState.FrontLeftPanel;
                Vehicle.Damage.Panels[1] = (byte)vehicleData.DamageState.FrontRightPanel;
                Vehicle.Damage.Panels[2] = (byte)vehicleData.DamageState.RearLeftPanel;
                Vehicle.Damage.Panels[3] = (byte)vehicleData.DamageState.RearRightPanel;
                Vehicle.Damage.Panels[4] = (byte)vehicleData.DamageState.Windscreen;
                Vehicle.Damage.Panels[5] = (byte)vehicleData.DamageState.FrontBumper;
                Vehicle.Damage.Panels[6] = (byte)vehicleData.DamageState.RearBumper;
                Vehicle.Damage.Doors[0] = (byte)vehicleData.DamageState.Hood;
                Vehicle.Damage.Doors[1] = (byte)vehicleData.DamageState.Trunk;
                Vehicle.Damage.Doors[2] = (byte)vehicleData.DamageState.FrontLeftDoor;
                Vehicle.Damage.Doors[3] = (byte)vehicleData.DamageState.FrontRightDoor;
                Vehicle.Damage.Doors[4] = (byte)vehicleData.DamageState.RearLeftDoor;
                Vehicle.Damage.Doors[5] = (byte)vehicleData.DamageState.RearRightDoor;
                Vehicle.DoorRatios = [ vehicleData.DoorOpenRatio.Hood, vehicleData.DoorOpenRatio.Trunk, vehicleData.DoorOpenRatio.FrontLeft,
                    vehicleData.DoorOpenRatio.FrontRight, vehicleData.DoorOpenRatio.RearLeft, vehicleData.DoorOpenRatio.RearRight ];
                Vehicle.SetWheelState(VehicleWheel.FrontLeft, (VehicleWheelState)vehicleData.WheelStatus.FrontLeft);
                Vehicle.SetWheelState(VehicleWheel.FrontRight, (VehicleWheelState)vehicleData.WheelStatus.FrontRight);
                Vehicle.SetWheelState(VehicleWheel.RearLeft, (VehicleWheelState)vehicleData.WheelStatus.RearLeft);
                Vehicle.SetWheelState(VehicleWheel.RearRight, (VehicleWheelState)vehicleData.WheelStatus.RearRight);
                Vehicle.IsEngineOn = vehicleData.EngineState;
                Vehicle.IsLandingGearDown = vehicleData.LandingGearDown;
                Vehicle.OverrideLights = (VehicleOverrideLights)vehicleData.OverrideLights;
                Vehicle.IsSirenActive = vehicleData.SirensState;
                Vehicle.IsLocked = vehicleData.Locked;
                Vehicle.IsTaxiLightOn = vehicleData.TaxiLightState;
                Vehicle.Health = vehicleData.Health;
                Vehicle.IsFrozen = vehicleData.IsFrozen;
            }

            foreach (var playerFeature in Vehicle.GetRequiredService<IEnumerable<IVehicleFeature>>())
            {
                if (playerFeature is IUsesVehiclePersistentData usesVehiclePersistentData)
                {
                    usesVehiclePersistentData.VersionIncreased += IncreaseVersion;
                    usesVehiclePersistentData.Loaded(vehicleData, preserveData);
                }
            }
            Loaded?.Invoke(this, Vehicle);
        }
    }

    public void IncreaseVersion()
    {
        Interlocked.Increment(ref _version);
    }

    public int GetVersion() => _version;

    public bool TryFlushVersion(int minimalVersion)
    {
        if (minimalVersion < 0)
            throw new ArgumentOutOfRangeException();

        if (_version == 0)
            return false;

        if (minimalVersion <= _version)
        {
            Interlocked.Exchange(ref _version, 0);
            return true;
        }
        return false;
    }

    public void UpdateLastUsed()
    {
        LastUsed = _dateTimeProvider.Now;
    }

    public void Unload()
    {
        lock (_lock)
        {
            if (IsLoaded)
            {
                foreach (var playerFeature in Vehicle.GetRequiredService<IEnumerable<IVehicleFeature>>())
                {
                    if (playerFeature is IUsesVehiclePersistentData usesVehiclePersistentData)
                    {
                        usesVehiclePersistentData.VersionIncreased -= IncreaseVersion;
                    }
                }
            }
        }
    }
}
