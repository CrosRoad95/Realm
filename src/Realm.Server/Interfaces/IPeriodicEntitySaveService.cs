namespace Realm.Server.Interfaces;

public interface IPeriodicEntitySaveService
{
    void AccountCreated(PlayerAccount playerAccount);
    void VehicleCreated(RPGVehicle persistantVehicle);
}
