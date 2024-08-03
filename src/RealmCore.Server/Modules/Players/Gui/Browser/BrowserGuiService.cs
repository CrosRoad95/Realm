namespace RealmCore.Server.Modules.Players.Gui.Browser;

public sealed class BrowserGuiService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, RealmPlayer> _playerByKey = new();
    private readonly List<RealmPlayer> _players = [];

    private readonly RandomNumberGenerator _randomNumberGenerator;
    private readonly byte[] _bytes = new byte[64];

    public BrowserGuiService()
    {
        _randomNumberGenerator = RandomNumberGenerator.Create();
    }

    public string GenerateKey()
    {
        lock (_lock)
        {
            _randomNumberGenerator.GetBytes(_bytes);

            return Convert.ToBase64String(_bytes).Replace('+', '-').Replace('/', '_');
        }
    }

    public bool IsAuthorized(RealmPlayer player)
    {
        lock (_lock)
        {
            return _players.Contains(player);
        }
    }

    public bool AuthorizePlayer(string key, RealmPlayer player)
    {
        lock (_lock)
        {
            if (_players.Contains(player) || _playerByKey.ContainsKey(key))
                return false;
            _players.Add(player);
            _playerByKey[key] = player;
            return true;
        }
    }

    public bool UnauthorizePlayer(RealmPlayer player)
    {
        lock (_lock)
        {
            if (!_players.Contains(player))
                return false;
            _players.Remove(player);
            _playerByKey.Remove(player.Browser.Key);
            return true;
        }
    }

    public bool TryGetPlayerByKey(string key, out RealmPlayer? player)
    {
        lock (_lock)
        {
            bool found = _playerByKey.TryGetValue(key, out player);

            return found;
        }
    }
}
