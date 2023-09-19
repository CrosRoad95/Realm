namespace RealmCore.Server.Interfaces;

public interface IScopedEntityFactory : IEntityFactoryBase, IDisposable
{
    Entity Entity { get; }
    IEnumerable<PlayerPrivateElementComponentBase> CreatedComponents { get; }
    PlayerPrivateElementComponentBase? LastCreatedComponent { get; }

    event Action<IScopedEntityFactory, PlayerPrivateElementComponentBase>? ComponentCreated;

    T GetLastCreatedComponent<T>() where T : PlayerPrivateElementComponentBase;
}
