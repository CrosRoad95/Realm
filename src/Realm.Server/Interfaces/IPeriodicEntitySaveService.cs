namespace Realm.Server.Interfaces;

public interface IPeriodicEntitySaveService
{
    void AccountCreated(PlayerAccount playerAccount);
    Task Flush();
    void VehicleCreated(RPGVehicle persistantVehicle);
}
