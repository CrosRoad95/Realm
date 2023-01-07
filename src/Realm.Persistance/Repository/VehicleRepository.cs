using Realm.Persistance.Extensions;
using Realm.Persistance.Interfaces;

namespace Realm.Persistance.Repository;

internal class VehicleRepository : IVehicleRepository
{
    private readonly IDb _db;

    public VehicleRepository(IDb db)
    {
        _db = db;
    }

    public async Task<Vehicle> CreateNewVehicle()
    {
        var vehicle = new Vehicle
        {
            UserId = Guid.NewGuid().ToString(),
            Platetext = Guid.NewGuid().ToString()[..8],
            CreatedAt = DateTime.Now,
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
