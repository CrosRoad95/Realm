using RealmCore.ECS;

namespace RealmCore.Server.Interfaces;

public interface ILoadService
{
    Task LoadAll();
    Task<Entity> LoadVehicleById(int id);
}
