using Realm.Common.Providers;

namespace Realm.Persistance.Repository;

internal class VehicleRepository : IVehicleRepository
{
    private readonly IDb _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public VehicleRepository(IDb db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Vehicle> CreateNewVehicle(ushort model)
    {
        var vehicle = new Vehicle
        {
            Model = model,
            Platetext = Guid.NewGuid().ToString()[..8],
            CreatedAt = _dateTimeProvider.Now,
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
            .Where(x => x.VehicleAccesses.Any(x => x.UserId == userId))
            .Select(x => new LightInfoVehicleDTO
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            });

        return query.ToListAsync();
    }
    
    public Task<List<Vehicle>> GetVehiclesByUserId(int userId)
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehicleRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.VehicleAccesses.Any(x => x.UserId == userId));

        return query.ToListAsync();
    }

    public Task<List<Vehicle>> GetAllSpawnedVehicles()
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehicleRepository))
            .AsNoTrackingWithIdentityResolution()
            .IncludeAll()
            .IsSpawned();

        return query.ToListAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    public Task Commit()
    {
        return _db.SaveChangesAsync();
    }
}
