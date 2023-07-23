using RealmCore.Web.AdminPanel.Classes;

namespace RealmCore.Web.AdminPanel.Services;

public class ServerService
{
    public event Action<Player>? PlayerJoined;
    public ServerService()
    {
    }

}
