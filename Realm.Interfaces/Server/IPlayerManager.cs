namespace Realm.Interfaces.Server;

public interface IPlayerManager
{
    event Action<IRPGPlayer>? PlayerJoined;
}
