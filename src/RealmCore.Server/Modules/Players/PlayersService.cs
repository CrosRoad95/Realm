namespace RealmCore.Server.Modules.Players;

public interface IPlayersService
{
    bool TryFindPlayerBySerial(string serial, out RealmPlayer? foundPlayer, PlayerSearchOption searchOptions = PlayerSearchOption.All);
    bool TryGetPlayerByName(string name, out RealmPlayer? foundPlayer, PlayerSearchOption searchOptions = PlayerSearchOption.All, RealmPlayer? ignored = null);
}

internal sealed class PlayersService : IPlayersService
{
    private readonly IElementCollection _elementCollection;

    public PlayersService(IElementCollection elementCollection)
    {
        _elementCollection = elementCollection;
    }

    public bool TryGetPlayerByName(string name, out RealmPlayer? foundPlayer, PlayerSearchOption searchOptions = PlayerSearchOption.All, RealmPlayer? ignored = null)
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

        if (ignored != null && player == ignored)
        {
            foundPlayer = null;
            return false;
        }

        foundPlayer = player;
        return true;
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
