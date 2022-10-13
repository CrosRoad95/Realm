namespace Realm.Interfaces.Server;

public interface IPlayerManager : ICommonManager
{
    event Action<IRPGPlayer>? PlayerJoined;
}
