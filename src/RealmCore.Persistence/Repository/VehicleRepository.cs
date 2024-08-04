namespace RealmCore.Persistence.Repository;

public sealed class VehicleRepository
{
    private readonly IDb _db;

    public VehicleRepository(IDb db)
    {
        _db = db;
    }

    public async Task<VehicleData> CreateVehicle(ushort model, DateTime now, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateVehicle));

        if (activity != null)
        {
            activity.AddTag("Model", model);
            activity.AddTag("Now", now);
        }

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

    public async Task<LightInfoVehicleDto[]> GetLightVehiclesByUserId(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetLightVehiclesByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.UserAccesses.Any(x => x.UserId == userId))
            .Select(x => new LightInfoVehicleDto
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            });

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<LightInfoVehicleDto?> GetLightVehicleById(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetLightVehicleById));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Id == vehicleId)
            .Select(x => new LightInfoVehicleDto
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            });

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<VehicleData[]> GetVehiclesByUserId(int userId, IEnumerable<int>? accessTypes = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetVehiclesByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("AccessTypes", accessTypes);
        }

        var query = BuildGetVehiclesByUserIdQuery(userId, accessTypes);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<int> CountVehiclesByUserId(int userId, IEnumerable<int>? accessTypes = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CountVehiclesByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("AccessTypes", accessTypes);
        }

        var query = BuildGetVehiclesByUserIdQuery(userId, accessTypes);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<VehicleData?> GetReadOnlyById(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetReadOnlyById));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int[]> GetAllSpawnedVehiclesIds(CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAllSpawnedVehiclesIds));

        var query = CreateQueryBase()
            .IsSpawned()
            .Select(x => x.Id);

        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<VehicleData?> GetById(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetById));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = CreateQueryBase()
            .Where(x => x.Id == id)
            .IncludeAll();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> SetSpawned(int id, bool spawned, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetSpawned));

        if (activity != null)
        {
            activity.AddTag("Id", id);
            activity.AddTag("Spawned", spawned);
        }

        var query = CreateQueryBase()
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

        if (activity != null)
        {
            activity.AddTag("Id", id);
            activity.AddTag("Kind", kind);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Id == id);

        var result = await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Kind, kind), cancellationToken);
        return result > 0;
    }

    public async Task<bool> IsSpawned(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(IsSpawned));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.Spawned);

        return await query.FirstAsync(cancellationToken);
    }

    public async Task<bool> SoftRemove(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SoftRemove));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Id == id);

        var result = await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.IsRemoved, true), cancellationToken);
        return result > 0;
    }

    public async Task<VehicleUserAccessData[]> GetAllVehicleAccesses(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAllVehicleAccesses));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = _db.VehicleUserAccess
            .TagWithSource(nameof(VehicleRepository))
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Where(x => x.VehicleId == id);
        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<bool> HasUserAccessTo(int userId, int vehicleId, byte[]? accessType = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(HasUserAccessTo));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("AccessType", accessType);
        }

        var query = _db.VehicleUserAccess
            .TagWithSource(nameof(VehicleRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.UserId == userId && (accessType == null || accessType.Contains(x.AccessType)))
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Select(x => x.UserId);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int[]> GetOwner(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetOwner));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query = _db.VehicleUserAccess
            .TagWithSource(nameof(VehicleRepository))
            .AsNoTracking()
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Where(x => x.VehicleId == vehicleId && x.AccessType == 0)
            .Select(x => x.UserId);
        return await query.ToArrayAsync(cancellationToken);
    }

    private IQueryable<VehicleData> BuildGetVehiclesByUserIdQuery(int userId, IEnumerable<int>? accessTypes)
    {
        if (accessTypes != null && !accessTypes.Any())
        {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        var query = CreateQueryBase()
            .AsNoTracking();

        if (accessTypes != null)
        {
            return query.Where(x => x.UserAccesses.Any(y => y.UserId == userId && accessTypes.Contains(y.AccessType)));
        }
        else
        {
            return query.Where(x => x.UserAccesses.Any(y => y.UserId == userId));
        }
    }

    private IQueryable<VehicleData> CreateQueryBase()
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved);

        return query;
    }

    public static readonly ActivitySource Activity = new("RealmCore.VehicleRepository", "1.0.0");
}
