namespace Realm.Persistance.Interfaces;

public interface IVehicleRepository : IDisposable
{
    Task<Vehicle> CreateNewVehicle();
    IQueryable<Vehicle> GetAll();
}
