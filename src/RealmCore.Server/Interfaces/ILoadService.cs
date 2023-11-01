namespace RealmCore.Server.Interfaces;

public interface ILoadService
{
    Task LoadAll();
    Task<RealmVehicle> LoadVehicleById(int id);
}
