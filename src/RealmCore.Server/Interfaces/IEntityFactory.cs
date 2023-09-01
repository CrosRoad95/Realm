namespace RealmCore.Server.Interfaces;

public interface IEntityFactory : IEntityFactoryBase
{
    IScopedEntityFactory CreateScopedEntityFactory(Entity entity);
}
