using RealmCore.WebApp.Classes;

namespace RealmCore.WebApp.Services;

public class ServerService
{
    public event Action<Player>? PlayerJoined;
    public ServerService()
    {
    }

}
