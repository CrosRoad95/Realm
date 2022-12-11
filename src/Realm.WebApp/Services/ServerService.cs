using Realm.WebApp.Classes;

namespace Realm.WebApp.Services;

public class ServerService
{
    public event Action<Player>? PlayerJoined;
    public ServerService()
    {
    }

}
