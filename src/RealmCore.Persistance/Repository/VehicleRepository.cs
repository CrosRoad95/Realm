namespace RealmCore.Persistance.Repository;

internal class VehicleRepository : IVehicleRepository
{
    private readonly IDb _db;

    public VehicleRepository(IDb db)
    {
        _db = db;
    }

    public async Task<VehicleData> CreateNewVehicle(ushort model, DateTime now)
    {
        var vehicle = new VehicleData
        {
            Model = model,
            Platetext = Guid.NewGuid().ToString()[..8],
            CreatedAt = now,
            Spawned = true,
        };
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();
        return vehicle;
    }

    public Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId)
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehicleRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.UserAccesses.Any(x => x.UserId == userId))
            .Select(x => new LightInfoVehicleDTO
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            });

        return query.ToListAsync();
    }

    public Task<List<VehicleData>> GetVehiclesByUserId(int userId)
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehicleRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.UserAccesses.Any(x => x.UserId == userId));

        return query.ToListAsync();
    }

    public Task<VehicleData?> GetReadOnlyVehicleById(int id)
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehicleRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == id);

        return query.FirstOrDefaultAsync();
    }

    public Task<List<VehicleData>> GetAllSpawnedVehicles()
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehicleRepository))
            .AsNoTrackingWithIdentityResolution()
            .IncludeAll()
            .IsSpawned();

        return query.ToListAsync();
    }

    public async Task<bool> SetSpawned(int id, bool spawned)
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => x.Id == id);

        var result = await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Spawned, spawned));
        return result > 0;
    }

    public async Task<bool> IsSpawned(int id)
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehicleRepository))
            .Where(x => x.Id == id)
            .Select(x => x.Spawned);

        return await query.FirstAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    public Task<int> Commit()
    {
        return _db.SaveChangesAsync();
    }
}
