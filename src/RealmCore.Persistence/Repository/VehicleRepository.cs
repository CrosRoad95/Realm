namespace RealmCore.Persistence.Repository;

public interface IVehicleRepository
{
    Task<VehicleData> CreateVehicle(ushort model, DateTime now, CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetAllSpawnedVehicles(CancellationToken cancellationToken = default);
    Task<List<VehicleUserAccessData>> GetAllVehicleAccesses(int vehicleId, CancellationToken cancellationToken = default);
    Task<LightInfoVehicleDTO?> GetLightVehicleById(int vehicleId, CancellationToken cancellationToken = default);
    Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId, CancellationToken cancellationToken = default);
    Task<List<int>> GetOwner(int vehicleId, CancellationToken cancellationToken = default);
    Task<VehicleData?> GetReadOnlyVehicleById(int id, CancellationToken cancellationToken = default);
    Task<VehicleData?> GetVehicleById(int id, CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetVehiclesByUserId(int userId, IEnumerable<int>? accessTypes = null, CancellationToken cancellationToken = default);
    Task<bool> HasUserAccessTo(int userId, int vehicleId, byte[]? accessType = null, CancellationToken cancellationToken = default);
    Task<bool> IsSpawned(int id, CancellationToken cancellationToken = default);
    Task<bool> SetKind(int id, byte kind, CancellationToken cancellationToken = default);
    Task<bool> SetSpawned(int id, bool spawned, CancellationToken cancellationToken = default);
    Task<bool> SoftRemove(int id, CancellationToken cancellationToken = default);
}

internal sealed class VehicleRepository : IVehicleRepository
{
    private readonly IDb _db;

    public VehicleRepository(IDb db)
    {
        _db = db;
    }

    public async Task<VehicleData> CreateVehicle(ushort model, DateTime now, CancellationToken cancellationToken = default)
    {
        var vehicle = new VehicleData
        {
            Model = model,
            Platetext = Guid.NewGuid().ToString()[..8],
            CreatedAt = now,
            Color = new(),
            DamageState = new(),
            DoorOpenRatio = new(),
            TransformAndMotion = new(),
            Variant = new(),
            WheelStatus = new(),
            Spawned = true,
        };
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync(cancellationToken);
        return vehicle;
    }

    public async Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.UserAccesses.Any(x => x.UserId == userId))
            .Select(x => new LightInfoVehicleDTO
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            });

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<LightInfoVehicleDTO?> GetLightVehicleById(int vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == vehicleId)
            .Select(x => new LightInfoVehicleDTO
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            });

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<VehicleData>> GetVehiclesByUserId(int userId, IEnumerable<int>? accessTypes = null, CancellationToken cancellationToken = default)
    {
        if(accessTypes != null && accessTypes.Count() == 0)
            throw new InvalidOperationException("Sequence contains no elements");

        IQueryable<VehicleData> query2;
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved);
        if (accessTypes != null)
            query2 = query.Where(x => x.UserAccesses.Any(y => y.UserId == userId && accessTypes.Contains(y.AccessType)));
        else
            query2 = query.Where(x => x.UserAccesses.Any(y => y.UserId == userId));


        return await query.ToListAsync(cancellationToken);
    }

    public async Task<VehicleData?> GetReadOnlyVehicleById(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<VehicleData>> GetAllSpawnedVehicles(CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .IncludeAll()
            .AsSplitQuery()
            .IsSpawned();

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<VehicleData?> GetVehicleById(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id)
            .IncludeAll();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> SetSpawned(int id, bool spawned, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id);

        var vehicle = await query.FirstAsync(cancellationToken);
        if (vehicle.Spawned == spawned)
            return false;

        vehicle.Spawned = spawned;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetKind(int id, byte kind, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id);

        var result = await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Kind, kind), cancellationToken);
        return result > 0;
    }

    public async Task<bool> IsSpawned(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id)
            .Select(x => x.Spawned);

        return await query.FirstAsync(cancellationToken);
    }

    public async Task<bool> SoftRemove(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id);

        var result = await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.IsRemoved, true), cancellationToken);
        return result > 0;
    }

    public async Task<List<VehicleUserAccessData>> GetAllVehicleAccesses(int vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _db.VehicleUserAccess
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Include(x => x.User)
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Where(x => x.VehicleId == vehicleId);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> HasUserAccessTo(int userId, int vehicleId, byte[]? accessType = null, CancellationToken cancellationToken = default)
    {
        var query = _db.VehicleUserAccess
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => x.VehicleId == vehicleId && x.UserId == userId && (accessType == null || accessType.Contains(x.AccessType)))
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Select(x => x.UserId);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<List<int>> GetOwner(int vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _db.VehicleUserAccess
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Where(x => x.VehicleId == vehicleId && x.AccessType == 0)
            .Select(x => x.UserId);
        return await query.ToListAsync(cancellationToken);
    }
}
