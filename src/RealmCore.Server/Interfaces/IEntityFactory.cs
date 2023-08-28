using RealmCore.Server.Interfaces.Abstractions;

namespace RealmCore.Server.Interfaces;

public interface IEntityFactory : IEntityFactoryBase
{
    Task<Entity> CreateNewPrivateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    IScopedEntityFactory CreateScopedEntityFactory(Entity entity);
}
