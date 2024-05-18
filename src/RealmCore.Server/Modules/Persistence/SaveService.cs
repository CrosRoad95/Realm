using static RealmCore.Persistence.Data.Helpers.VehicleDamageState;
using static RealmCore.Persistence.Data.Helpers.VehicleWheelStatus;

namespace RealmCore.Server.Modules.Persistence;

public interface IElementSaveService
{
    event Action<Element>? ElementSaved;

    Task<bool> Save(CancellationToken cancellationToken = default);
    Task SaveNewInventory(Inventory inventory, CancellationToken cancellationToken = default);
}

internal sealed partial class ElementSaveService : IElementSaveService
{
    private readonly IDb _db;
    private readonly IEnumerable<IUserDataSaver> _userDataSavers;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IInventoryRepository _inventoryRepository;

    public event Action<Element>? ElementSaved;
    private readonly Element _element;

    public ElementSaveService(IDb db, IEnumerable<IUserDataSaver> userDataSavers, IDateTimeProvider dateTimeProvider, ElementContext elementContext, IInventoryRepository inventoryRepository)
    {
        _db = db;
        _userDataSavers = userDataSavers;
        _dateTimeProvider = dateTimeProvider;
        _inventoryRepository = inventoryRepository;
        _element = elementContext.Element;
    }

    private async Task<bool> SaveVehicle(RealmVehicle vehicle, CancellationToken cancellationToken = default)
    {
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
            vehicleData.Inventories = SaveInventory(vehicleData.Inventories, inventory);

        vehicleData.LastUsed = vehicle.Persistence.LastUsed;

        var savedEntities = await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> SavePlayer(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (!player.User.IsLoggedIn)
            return false;

        var userData = player.User.UserData;

        if(player.IsSpawned)
            userData.LastTransformAndMotion = player.GetTransformAndMotion();

        player.PlayTime.UpdateCategoryPlayTime(player.PlayTime.Category);
        userData.PlayTime = (ulong)player.PlayTime.TotalPlayTime.TotalSeconds;

        if (player.Inventory.TryGetPrimary(out var inventory))
            userData.Inventories = SaveInventory(userData.Inventories, inventory);

        foreach (var item in _userDataSavers)
            await item.SaveAsync(userData, player, cancellationToken);

        var savedEntities = await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private ICollection<InventoryData> SaveInventory(ICollection<InventoryData> inventoriesData, Inventory inventory)
    {
        var existingInventoryData = inventoriesData.FirstOrDefault(x => x.Id == inventory.Id);
        if (existingInventoryData == null)
        {
            inventoriesData = [Inventory.CreateData(inventory)];
        }
        else
        {
            foreach (var item in inventory.Items)
            {
                var existingItem = existingInventoryData.InventoryItems.FirstOrDefault(x => x.Id == item.Id);
                var itemData = Item.CreateData(item);
                if (existingItem == null)
                {
                    existingInventoryData.InventoryItems.Add(itemData);
                }
                else
                {
                    existingItem.MetaData = itemData.MetaData;
                    existingItem.Number = itemData.Number;
                    existingItem.ItemId = itemData.ItemId;
                }
            }

            var removedItems = existingInventoryData.InventoryItems.Select(x => x.Id)
                .Except(inventory.Items.Select(x => x.Id))
                .ToArray();

            foreach (var removedItem in removedItems)
            {
                existingInventoryData.InventoryItems.Remove(existingInventoryData.InventoryItems.First(x => x.Id == removedItem));
            }
        }

        return inventoriesData;
    }

    public async Task<bool> Save(CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Save));
        try
        {
            bool saved = _element switch
            {
                RealmPlayer player => await SavePlayer(player, cancellationToken),
                RealmVehicle vehicle => await SaveVehicle(vehicle, cancellationToken),
                _ => false
            };

            if (saved)
                ElementSaved?.Invoke(_element);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return saved;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.ToString());
            throw;
        }
    }

    public async Task SaveNewInventory(Inventory inventory, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SaveNewInventory));

        try
        {
            int inventoryId = _element switch
            {
                RealmPlayer player => await SaveNewPlayerInventory(inventory, player.UserId, cancellationToken),
                RealmVehicle vehicle => await SaveNewVehicleInventory(inventory, vehicle.VehicleId, cancellationToken),
                _ => 0
            };

            if (inventoryId == 0)
                throw new FailedToSaveInventoryException();

            inventory.Id = inventoryId;
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch(Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.ToString());
            throw;
        }
    }

    private async Task<int> SaveNewPlayerInventory(Inventory inventory, int userId, CancellationToken cancellationToken = default)
    {
        var inventoryData = Inventory.CreateData(inventory);
        return await _inventoryRepository.CreateInventoryForUserId(userId, inventoryData, cancellationToken);
    }

    private async Task<int> SaveNewVehicleInventory(Inventory inventory, int vehicleId, CancellationToken cancellationToken = default)
    {
        var inventoryData = Inventory.CreateData(inventory);
        return await _inventoryRepository.CreateInventoryForVehicleId(vehicleId, inventoryData, cancellationToken);
    }

    public static readonly ActivitySource Activity = new("RealmCore.SaveService", "1.0.0");
}
