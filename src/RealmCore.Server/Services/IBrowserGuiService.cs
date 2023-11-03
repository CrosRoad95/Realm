namespace RealmCore.Server.Services;

public interface IBrowserGuiService
{
    string KeyName { get; }

    event Action<RealmPlayer>? Ready;

    string GenerateKey();
    bool AuthorizePlayer(string key, RealmPlayer player);
    bool UnauthorizePlayer(RealmPlayer player);
    bool TryGetPlayerByKey(string key, out RealmPlayer? player);
    bool TryGetKeyByPlayer(RealmPlayer player, out string? key);
    void RelayPlayerLoggedIn(RealmPlayer player);
}
