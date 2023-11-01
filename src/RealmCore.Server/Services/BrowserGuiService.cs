using System.Security.Cryptography;

namespace RealmCore.Server.Services;

internal class BrowserGuiService : IBrowserGuiService
{
    private readonly ConcurrentDictionary<string, RealmPlayer> _browserPlayers = new();

    private readonly RandomNumberGenerator _randomNumberGenerator;
    private readonly object _lock = new();
    private readonly byte[] _bytes = new byte[64];
    public event Action<RealmPlayer>? Ready;
    public string KeyName => "guiKey";

    public BrowserGuiService()
    {
        _randomNumberGenerator = RandomNumberGenerator.Create();
    }

    public void RelayPlayerLoggedIn(RealmPlayer realmPlayer)
    {
        Ready?.Invoke(realmPlayer);
    }

    public string GenerateKey()
    {
        lock (_lock)
        {
            _randomNumberGenerator.GetBytes(_bytes);

            return Convert.ToBase64String(_bytes).Replace('+', '-').Replace('/', '_');
        }
    }
    public void AuthorizePlayer(string key, RealmPlayer realmPlayer)
    {
        _browserPlayers.TryAdd(key, realmPlayer);
    }

    public void UnauthorizePlayer(RealmPlayer realmPlayer)
    {
        var itemsToRemove = _browserPlayers.Where(x => x.Value == realmPlayer).FirstOrDefault();

        _browserPlayers.TryRemove(itemsToRemove.Key, out var _);
    }
    
    public bool TryGetKeyByPlayer(RealmPlayer realmPlayer, out string? key)
    {
        var browserPlayers = _browserPlayers.Where(x => x.Value == realmPlayer).ToList();
        if (browserPlayers.Count == 0)
        {
            key = null;
            return false;
        }
        key = browserPlayers[0].Key;
        return true;
    }

    public bool TryGetPlayerByKey(string key, out RealmPlayer? realmPlayer)
    {
        bool found = _browserPlayers.TryGetValue(key, out realmPlayer);

        return found;
    }
}
