using Realm.Interfaces.Scripting.Classes;
using Realm.Interfaces.Server;

namespace Realm.Interfaces;

public interface IMtaServer
{
    event Action<IRPGPlayer>? PlayerJoined;
}
