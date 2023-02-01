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
            Id = Guid.NewGuid(),
            Model = model,
            Platetext = Guid.NewGuid().ToString()[..8],
            CreatedAt = DateTime.Now,
            Spawned = true,
        };
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();
        return vehicle;
    }

    public IQueryable<Vehicle> GetAll()
    {
        return _db.Vehicles.IsNotRemoved();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
