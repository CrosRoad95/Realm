
namespace RealmCore.Server.Services;

public interface IBrowserGuiService
{
    string KeyName { get; }

    event Action<RealmPlayer>? Ready;

    string GenerateKey();
    void AuthorizePlayer(string key, RealmPlayer realmPlayer);
    void UnauthorizePlayer(RealmPlayer realmPlayer);
    bool TryGetPlayerByKey(string key, out RealmPlayer? realmPlayer);
    bool TryGetKeyByPlayer(RealmPlayer realmPlayer, out string? key);
    void RelayPlayerLoggedIn(RealmPlayer realmPlayer);
}
