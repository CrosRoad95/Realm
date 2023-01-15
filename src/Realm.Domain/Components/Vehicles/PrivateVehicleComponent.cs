using VehicleData = Realm.Persistance.Data.Vehicle;
using VehicleAccess = Realm.Domain.Concepts.VehicleAccess;

namespace Realm.Domain.Components.Vehicles;

public class PrivateVehicleComponent : Component
{
    private readonly VehicleData _vehicleData;

    internal VehicleData VehicleData => _vehicleData;
    internal Guid Id => _vehicleData.Id;

    private List<VehicleAccess> _vehicleAccesses = new();
    public IEnumerable<VehicleAccess> VehicleAccesses => _vehicleAccesses;

    internal PrivateVehicleComponent(VehicleData vehicleData)
    {
        _vehicleData = vehicleData;
    }

    public override Task Load()
    {
        var vehicleElementComponent = Entity.GetRequiredComponent<VehicleElementComponent>();
        var vehicle = vehicleElementComponent.Vehicle;
        vehicle.Colors.Primary = _vehicleData.Color.Color1;
        vehicle.Colors.Secondary = _vehicleData.Color.Color2;
        vehicle.Colors.Color3 = _vehicleData.Color.Color3;
        vehicle.Colors.Color4 = _vehicleData.Color.Color4;
        vehicle.PaintJob = (byte)_vehicleData.Paintjob;
        vehicle.PlateText = _vehicleData.Platetext;
        vehicle.Variants = new SlipeServer.Server.ElementConcepts.VehicleVariants
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
        _vehicleAccesses = _vehicleData.VehicleAccesses.Select(x => new VehicleAccess
        {
            Id = x.Id,
            UserId = x.User.Id,
            Ownership = x.Description.Ownership
        }).ToList();

        return Task.CompletedTask;
    }

    public bool TryGetAccess(Entity entity, out VehicleAccess vehicleAccess)
    {
        var userId = entity.GetRequiredComponent<AccountComponent>().Id;
        var index = _vehicleAccesses.FindIndex(x => x.UserId == userId);
        if(index >= 0)
        {
            vehicleAccess = _vehicleAccesses[index];
            return true;
        }
        vehicleAccess = default;
        return false;
    }

    public VehicleAccess AddOwner(Entity entity)
    {
        if (TryGetAccess(entity, out var _))
            throw new EntityAccessDefinedException();

        var vehicleAccess = new VehicleAccess
        {
            UserId = entity.GetRequiredComponent<AccountComponent>().Id,
            Ownership = true,
        };
        _vehicleAccesses.Add(vehicleAccess);
        return vehicleAccess;
    }
}
