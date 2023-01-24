namespace Realm.Persistance.Interfaces;

public interface IVehicleRepository : IDisposable
{
    Task<Vehicle> CreateNewVehicle(ushort model);
    IQueryable<Vehicle> GetAll();
}
