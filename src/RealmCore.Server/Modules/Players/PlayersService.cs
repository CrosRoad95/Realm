namespace RealmCore.Server.Modules.Players;

[Flags]
public enum PlayerSearchOption
{
    None = 0,
    CaseInsensitive = 1,
    LoggedIn = 2,
    All = CaseInsensitive | LoggedIn
}

public interface IPlayersService
{
    IEnumerable<RealmPlayer> SearchPlayersByName(string pattern, PlayerSearchOption searchOptions = PlayerSearchOption.All, RealmPlayer? ignore = null);
    bool TryFindPlayerBySerial(string serial, out RealmPlayer? foundPlayer, PlayerSearchOption searchOptions = PlayerSearchOption.All);
    bool TryGetPlayerByName(string name, out RealmPlayer? foundPlayer, PlayerSearchOption searchOptions = PlayerSearchOption.All);
}

internal sealed class PlayersService : IPlayersService
{
    private readonly IElementCollection _elementCollection;

    public PlayersService(IElementCollection elementCollection)
    {
        _elementCollection = elementCollection;
    }

    public bool TryGetPlayerByName(string name, out RealmPlayer? foundPlayer, PlayerSearchOption searchOptions = PlayerSearchOption.All)
    {
        var player = _elementCollection.GetByType<RealmPlayer>()
            .Where(x => string.Equals(x.Name, name, searchOptions.HasFlag(PlayerSearchOption.CaseInsensitive) ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture))
            .FirstOrDefault();
        if (player == null)
        {
            foundPlayer = null;
            return false;
        }

        if (searchOptions.HasFlag(PlayerSearchOption.LoggedIn))
        {
            if (!player.User.IsSignedIn)
            {
                foundPlayer = null;
                return false;
            }
        }

        foundPlayer = player;
        return true;
    }

    public IEnumerable<RealmPlayer> SearchPlayersByName(string pattern, PlayerSearchOption searchOptions = PlayerSearchOption.All, RealmPlayer? ignore = null)
    {
        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
        {
            if (ignore != null && ignore == player)
                continue;

            if (searchOptions.HasFlag(PlayerSearchOption.LoggedIn))
            {
                if (!player.User.IsSignedIn)
                    continue;
            }

            if (player.Name.Contains(pattern.ToLower(), StringComparison.CurrentCultureIgnoreCase))
                yield return player;
        }
    }

    public bool TryFindPlayerBySerial(string serial, out RealmPlayer? foundPlayer, PlayerSearchOption searchOptions = PlayerSearchOption.All)
    {
        if (serial.Length != 32)
            throw new ArgumentException(nameof(serial));

        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
        {
            if (string.Equals(player.Client.Serial, serial, searchOptions.HasFlag(PlayerSearchOption.CaseInsensitive) ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture))
            {
                if (searchOptions.HasFlag(PlayerSearchOption.LoggedIn))
                {
                    if (!player.User.IsSignedIn)
                    {
                        foundPlayer = null;
                        return false;
                    }
                }
                foundPlayer = player;
                return true;
            }
        }
        foundPlayer = null;
        return false;
    }
}
