namespace Realm.Persistance.Repository;

internal class VehicleRepository : IVehicleRepository
{
    private readonly IDb _db;

    public VehicleRepository(IDb db)
    {
        _db = db;
    }

    public async Task<Vehicle> CreateNewVehicle(ushort model)
    {
        var vehicle = new Vehicle
        {
            Model = model,
            Platetext = Guid.NewGuid().ToString()[..8],
            CreatedAt = DateTime.Now,
            Spawned = true,
        };
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();
        return vehicle;
    }

    public Task<List<VehicleModelPositionDTO>> GetAllVehiclesModelPositionDTOsByUserId(int userId)
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehicleRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.VehicleAccesses)
            .Where(x => x.VehicleAccesses.Any(x => x.UserId == userId))
            .Select(x => new VehicleModelPositionDTO
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            });

        return query.ToListAsync();
    }

    public Task<List<Vehicle>> GetAllReadOnlySpawnedVehicles()
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
