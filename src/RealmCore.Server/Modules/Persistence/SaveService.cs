using static RealmCore.Persistence.Data.Helpers.VehicleDamageState;
using static RealmCore.Persistence.Data.Helpers.VehicleWheelStatus;

namespace RealmCore.Server.Modules.Persistence;

public interface ISaveService
{
    event Action<Element>? ElementSaved;

    Task<bool> Save(Element element, bool firstTime = false, CancellationToken cancellationToken = default);
    internal Task<int> SaveNewPlayerInventory(Inventory inventory, int userId, CancellationToken cancellationToken = default);
    internal Task<int> SaveNewVehicleInventory(Inventory inventory, int vehicleId, CancellationToken cancellationToken = default);
}

internal sealed class SaveService : ISaveService
{
    private readonly IDb _dbContext;
    private readonly IEnumerable<IUserDataSaver> _userDataSavers;

    public event Action<Element>? ElementSaved;

    public SaveService(IDb dbContext, IEnumerable<IUserDataSaver> userDataSavers)
    {
        _dbContext = dbContext;
        _userDataSavers = userDataSavers;
    }

    private async Task<bool> SaveVehicle(RealmVehicle vehicle, bool firstTime = false, CancellationToken cancellationToken = default)
    {
        if (!vehicle.Persistence.IsLoaded)
            return false;

        var vehicleData = vehicle.Persistence.VehicleData;

        vehicleData.TransformAndMotion = vehicle.GetTransformAndMotion();
        vehicleData.Mileage = vehicle.MileageCounter.Mileage;
        vehicleData.Model = vehicle.Model;
        vehicleData.Color = new VehicleColor(vehicle.Colors.Primary, vehicle.Colors.Secondary, vehicle.Colors.Color3, vehicle.Colors.Color4, vehicle.HeadlightColor);
        vehicleData.Paintjob = vehicle.PaintJob;
        vehicleData.Platetext = vehicle.PlateText;
        vehicleData.Variant = new VehicleVariant(vehicle.Variants.Variant1, vehicle.Variants.Variant2);
        vehicleData.DamageState = new VehicleDamageState
        {
            FrontLeftLight = (LightState)vehicle.Damage.Lights[0],
            FrontRightLight = (LightState)vehicle.Damage.Lights[1],
            RearLeftLight = (LightState)vehicle.Damage.Lights[2],
            RearRightLight = (LightState)vehicle.Damage.Lights[3],
            FrontLeftPanel = (PanelState)vehicle.Damage.Panels[0],
            FrontRightPanel = (PanelState)vehicle.Damage.Panels[1],
            RearLeftPanel = (PanelState)vehicle.Damage.Panels[2],
            RearRightPanel = (PanelState)vehicle.Damage.Panels[3],
            Windscreen = (PanelState)vehicle.Damage.Panels[4],
            FrontBumper = (PanelState)vehicle.Damage.Panels[5],
            RearBumper = (PanelState)vehicle.Damage.Panels[6],
            Hood = (DoorState)vehicle.Damage.Doors[0],
            Trunk = (DoorState)vehicle.Damage.Doors[1],
            FrontLeftDoor = (DoorState)vehicle.Damage.Doors[2],
            FrontRightDoor = (DoorState)vehicle.Damage.Doors[3],
            RearLeftDoor = (DoorState)vehicle.Damage.Doors[4],
            RearRightDoor = (DoorState)vehicle.Damage.Doors[5],
        };
        vehicleData.DoorOpenRatio = new VehicleDoorOpenRatio
        {
            Hood = vehicle.DoorRatios[0],
            Trunk = vehicle.DoorRatios[1],
            FrontLeft = vehicle.DoorRatios[2],
            FrontRight = vehicle.DoorRatios[3],
            RearLeft = vehicle.DoorRatios[4],
            RearRight = vehicle.DoorRatios[5],
        };
        vehicleData.WheelStatus = new VehicleWheelStatus
        {
            FrontLeft = (WheelStatus)vehicle.GetWheelState(VehicleWheel.FrontLeft),
            FrontRight = (WheelStatus)vehicle.GetWheelState(VehicleWheel.FrontRight),
            RearLeft = (WheelStatus)vehicle.GetWheelState(VehicleWheel.RearLeft),
            RearRight = (WheelStatus)vehicle.GetWheelState(VehicleWheel.RearLeft),
        };
        vehicleData.EngineState = vehicle.IsEngineOn;
        vehicleData.LandingGearDown = vehicle.IsLandingGearDown;
        vehicleData.OverrideLights = (byte)vehicle.OverrideLights;
        vehicleData.SirensState = vehicle.IsSirenActive;
        vehicleData.Locked = vehicle.IsLocked;
        vehicleData.TaxiLightState = vehicle.IsTaxiLightOn;
        vehicleData.Health = vehicle.Health;
        vehicleData.IsFrozen = vehicle.IsFrozen;
        vehicleData.TransformAndMotion = vehicle.GetTransformAndMotion();
        vehicleData.UserAccesses = vehicle.Access.Select(x => new VehicleUserAccessData
        {
            Id = x.Id,
            UserId = x.UserId,
            VehicleId = vehicleData.Id,
            Vehicle = vehicleData,
            AccessType = x.AccessType,
            CustomValue = x.CustomValue
        }).ToList();

        vehicleData.Paintjob = vehicle.PaintJob;

        if (vehicle.Inventory.TryGetPrimary(out var inventory))
            vehicleData.Inventories = [Inventory.CreateData(inventory)];
        vehicleData.LastUsed = vehicle.Persistence.LastUsed;
        await vehicle.ServiceProvider.GetRequiredService<IDb>().SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> SaveNewPlayerInventory(Inventory inventory, int userId, CancellationToken cancellationToken = default)
    {
        var inventoryData = Inventory.CreateData(inventory);
        _dbContext.UserInventories.Add(new UserInventoryData
        {
            Inventory = inventoryData,
            UserId = userId
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return inventory.Id;
    }

    public async Task<int> SaveNewVehicleInventory(Inventory inventory, int vehicleId, CancellationToken cancellationToken = default)
    {
        var inventoryData = Inventory.CreateData(inventory);
        _dbContext.VehicleInventories.Add(new VehicleInventoryData
        {
            Inventory = inventoryData,
            VehicleId = vehicleId
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return inventory.Id;
    }

    private async Task<bool> SavePlayer(RealmPlayer player, bool firstTime = false, CancellationToken cancellationToken = default)
    {
        if (!player.User.IsSignedIn)
            return false;

        var user = player.User.UserData;
        var db = player.GetRequiredService<IDb>();

        if(player.IsSpawned)
            user.LastTransformAndMotion = player.GetTransformAndMotion();
        user.PlayTime = (ulong)player.PlayTime.TotalPlayTime.TotalSeconds;

        db.Users.Update(user);

        foreach (var item in _userDataSavers)
            await item.SaveAsync(player, firstTime, cancellationToken);

        var savedEntities = await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> Save(Element element, bool firstTime = false, CancellationToken cancellationToken = default)
    {
        bool saved = element switch
        {
            RealmPlayer player => await SavePlayer(player, firstTime, cancellationToken),
            RealmVehicle vehicle => await SaveVehicle(vehicle, firstTime, cancellationToken),
            _ => false
        };

        if (saved)
            ElementSaved?.Invoke(element);
        return saved;
    }
}
