namespace RealmCore.Persistence.Repository;

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
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
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

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<VehicleData>> GetVehiclesByUserId(int userId, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.UserAccesses.Any(x => x.UserId == userId));

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<VehicleData?> GetReadOnlyVehicleById(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<VehicleData>> GetAllSpawnedVehicles(CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .IncludeAll()
            .IsSpawned();

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<VehicleData?> GetVehicleById(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id)
            .IncludeAll();

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> SetSpawned(int id, bool spawned, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id);

        var result = await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Spawned, spawned), cancellationToken).ConfigureAwait(false);
        return result > 0;
    }

    public async Task<bool> SetKind(int id, byte kind, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id);

        var result = await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Kind, kind), cancellationToken).ConfigureAwait(false);
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

        return await query.FirstAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> SoftRemove(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Vehicles
            .AsNoTracking()
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => !x.IsRemoved)
            .Where(x => x.Id == id);

        var result = await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.IsRemoved, true), cancellationToken).ConfigureAwait(false);
        return result > 0;
    }

    public async Task<List<VehicleUserAccessData>> GetAllVehicleAccesses(int vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _db.VehicleUserAccess
            .AsNoTracking()
            .TagWithSource(nameof(VehicleEventRepository))
            .Include(x => x.User)
            .Where(x => x.VehicleId == vehicleId);
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
