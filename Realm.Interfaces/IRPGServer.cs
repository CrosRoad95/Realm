namespace Realm.Interfaces;

public interface IRPGServer
{
    event Action<IRPGPlayer>? PlayerJoined;
}
