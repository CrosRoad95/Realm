namespace Realm.Interfaces;

public interface IMtaServer
{
    event Action<IRPGPlayer>? PlayerJoined;
}
