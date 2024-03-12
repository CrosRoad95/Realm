namespace RealmCore.Persistence.Repository;

public interface IVehicleRepository
{
    Task<int> CountVehiclesByUserId(int userId, IEnumerable<int>? accessTypes = null, CancellationToken cancellationToken = default);
    Task<VehicleData> CreateVehicle(ushort model, DateTime now, CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetAllSpawnedVehicles(CancellationToken cancellationToken = default);
    Task<List<VehicleUserAccessData>> GetAllVehicleAccesses(int vehicleId, CancellationToken cancellationToken = default);
    Task<LightInfoVehicleDto?> GetLightVehicleById(int vehicleId, CancellationToken cancellationToken = default);
    Task<List<LightInfoVehicleDto>> GetLightVehiclesByUserId(int userId, CancellationToken cancellationToken = default);
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
        using var activity = Activity.StartActivity(nameof(CreateVehicle));

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

    public async Task<List<LightInfoVehicleDto>> GetLightVehiclesByUserId(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetLightVehiclesByUserId));

        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.UserAccesses.Any(x => x.UserId == userId))
            .Select(x => new LightInfoVehicleDto
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            });

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<LightInfoVehicleDto?> GetLightVehicleById(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetLightVehicleById));

        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == vehicleId)
            .Select(x => new LightInfoVehicleDto
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            });

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<List<VehicleData>> GetVehiclesByUserId(int userId, IEnumerable<int>? accessTypes = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetVehiclesByUserId));

        var query = BuildGetVehiclesByUserIdQuery(userId, accessTypes);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> CountVehiclesByUserId(int userId, IEnumerable<int>? accessTypes = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CountVehiclesByUserId));

        var query = BuildGetVehiclesByUserIdQuery(userId, accessTypes);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<VehicleData?> GetReadOnlyVehicleById(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetReadOnlyVehicleById));

        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<VehicleData>> GetAllSpawnedVehicles(CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAllSpawnedVehicles));

        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .IncludeAll()
            .IsSpawned();

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<VehicleData?> GetVehicleById(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetVehicleById));

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
        using var activity = Activity.StartActivity(nameof(SetSpawned));

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
        using var activity = Activity.StartActivity(nameof(SetKind));

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
        using var activity = Activity.StartActivity(nameof(IsSpawned));

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
        using var activity = Activity.StartActivity(nameof(SoftRemove));

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
        using var activity = Activity.StartActivity(nameof(GetAllVehicleAccesses));

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
        using var activity = Activity.StartActivity(nameof(HasUserAccessTo));

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
        using var activity = Activity.StartActivity(nameof(GetOwner));

        var query = _db.VehicleUserAccess
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Where(x => x.VehicleId == vehicleId && x.AccessType == 0)
            .Select(x => x.UserId);
        return await query.ToListAsync(cancellationToken);
    }

    private IQueryable<VehicleData> BuildGetVehiclesByUserIdQuery(int userId, IEnumerable<int>? accessTypes)
    {
        using var activity = Activity.StartActivity(nameof(BuildGetVehiclesByUserIdQuery));

        if (accessTypes != null && !accessTypes.Any())
        {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved);

        if (accessTypes != null)
        {
            return query.Where(x => x.UserAccesses.Any(y => y.UserId == userId && accessTypes.Contains(y.AccessType)));
        }
        else
        {
            return query.Where(x => x.UserAccesses.Any(y => y.UserId == userId));
        }
    }

    public static readonly ActivitySource Activity = new("RealmCore.VehicleRepository", "1.0.0");
}
