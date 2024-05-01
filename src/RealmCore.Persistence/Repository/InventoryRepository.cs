namespace RealmCore.Persistence.Repository;

public interface IInventoryRepository
{
    Task<int> CreateInventoryForUserId(int userId, InventoryData inventoryData, CancellationToken cancellationToken = default);
    Task<int> CreateInventoryForUserId(int userId, int size, CancellationToken cancellationToken = default);
    Task<int> CreateInventoryForVehicleId(int vehicleId, InventoryData inventoryData, CancellationToken cancellationToken = default);
    Task<int> CreateInventoryForVehicleId(int vehicleId, int size, CancellationToken cancellationToken = default);
    Task<UserInventoryData[]> GetAllInventoriesByUserId(int userId, CancellationToken cancellationToken = default);
    Task<VehicleInventoryData[]> GetAllInventoriesByVehicleId(int vehicleId, CancellationToken cancellationToken = default);
    Task<InventoryData?> GetInventoryById(int inventoryId, CancellationToken cancellationToken = default);
}

internal sealed class InventoryRepository : IInventoryRepository
{
    private readonly IDb _db;

    public InventoryRepository(IDb db)
    {
        _db = db;
    }

    public async Task<int> CreateInventoryForUserId(int userId, InventoryData inventoryData, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateInventoryForUserId));
        if(activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var userInventoryData = new UserInventoryData
        {
            Inventory = inventoryData,
            UserId = userId
        };

        _db.UserInventories.Add(userInventoryData);
        await _db.SaveChangesAsync(cancellationToken);
        return inventoryData.Id;
    }
    
    public async Task<int> CreateInventoryForUserId(int userId, int size, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateInventoryForUserId));
        if(activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var userInventoryData = new UserInventoryData
        {
            Inventory = new InventoryData
            {
                Size = size
            },
            UserId = userId
        };

        _db.UserInventories.Add(userInventoryData);
        await _db.SaveChangesAsync(cancellationToken);
        return userInventoryData.Inventory.Id;
    }
    
    public async Task<UserInventoryData[]> GetAllInventoriesByUserId(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAllInventoriesByUserId));
        if(activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.UserInventories.Where(x => x.UserId == userId);
        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<int> CreateInventoryForVehicleId(int vehicleId, InventoryData inventoryData, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateInventoryForUserId));
        if(activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var vehicleInventoryData = new VehicleInventoryData
        {
            Inventory = inventoryData,
            VehicleId = vehicleId
        };

        _db.VehicleInventories.Add(vehicleInventoryData);
        await _db.SaveChangesAsync(cancellationToken);
        return inventoryData.Id;
    }
    
    public async Task<int> CreateInventoryForVehicleId(int vehicleId, int size, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateInventoryForVehicleId));
        if(activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var vehicleInventoryData = new VehicleInventoryData
        {
            Inventory = new InventoryData
            {
                Size = size
            },
            VehicleId = vehicleId
        };

        _db.VehicleInventories.Add(vehicleInventoryData);
        await _db.SaveChangesAsync(cancellationToken);
        return vehicleInventoryData.Inventory.Id;
    }
    
    public async Task<VehicleInventoryData[]> GetAllInventoriesByVehicleId(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAllInventoriesByUserId));
        if(activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query = _db.VehicleInventories.Where(x => x.VehicleId == vehicleId);
        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<InventoryData?> GetInventoryById(int inventoryId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetInventoryById));
        if (activity != null)
        {
            activity.AddTag("InventoryId", inventoryId);
        }

        var query = _db.Inventories.Where(x => x.Id == inventoryId)
            .Include(x => x.InventoryItems);

        return await query.FirstAsync(cancellationToken);
    }

    public static readonly ActivitySource Activity = new("RealmCore.InventoryRepository", "1.0.0");
}
