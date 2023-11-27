namespace RealmCore.Server.Interfaces;

public interface ILoadService
{
    Task LoadAll(CancellationToken cancellationToken = default);
    Task<RealmVehicle> LoadVehicleById(int id, CancellationToken cancellationToken = default);
}
