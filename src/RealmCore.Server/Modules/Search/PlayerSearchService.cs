namespace RealmCore.Server.Modules.Search;

public record struct VehicleSearchOptions(VehicleSearchOption searchOptions = VehicleSearchOption.All, RealmVehicle? ignore = null);

public record struct PlayerSearchOptions(PlayerSearchOption searchOptions = PlayerSearchOption.All, RealmPlayer? ignore = null);

public sealed class PlayerSearchService
{
    private readonly IElementCollection _elementCollection;
    private readonly RealmPlayer _player;

    public PlayerSearchService(PlayerContext playerContext, IElementCollection elementCollection)
    {
        _player = playerContext.Player;
        _elementCollection = elementCollection;
    }

    public RealmPlayer? FindPlayer(string pattern, PlayerSearchOptions options = default)
    {
        var players = SearchPlayers(pattern, options).ToArray();

        if (players.Length == 1)
            return players[0];

        return null;
    }
    
    public RealmVehicle? FindVehicle(string pattern, VehicleSearchOptions options = default)
    {
        var vehicles = SearchVehicles(pattern, options).ToArray();

        if (vehicles.Length == 1)
            return vehicles[0];

        return null;
    }
    
    public IEnumerable<RealmPlayer> SearchPlayers(string pattern, PlayerSearchOptions options = default)
    {
        foreach (var vehicle in SearchPlayersCore(pattern, options))
        {
            if (options.ignore != null && options.ignore == vehicle)
                continue;
            yield return vehicle;
        }
    }
    
    public IEnumerable<RealmVehicle> SearchVehicles(string pattern, VehicleSearchOptions options = default)
    {
        foreach (var vehicle in SearchVehiclesCore(pattern, options.searchOptions))
        {
            if (options.ignore != null && options.ignore == vehicle)
                continue;
            yield return vehicle;
        }
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
            if (!player.User.IsLoggedIn)
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
                    if (!player.User.IsLoggedIn)
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

    private IEnumerable<RealmVehicle> SearchVehiclesCore(string pattern, VehicleSearchOption searchOptions = VehicleSearchOption.All)
    {
        var match = Regexes.SpecialSearchSwitch().Match(pattern);
        if (!match.Success)
            yield break;

        var search = match.Groups["Search"].Value;
        if (!Enum.TryParse<SearchSwitch>(search, true, out var searchSwitch))
            throw new UnknownSearchSwitchException(match.Groups["Character"].Value);

        var allExceptMe = char.IsUpper(search, 0);
        switch (searchSwitch)
        {
            case SearchSwitch.N:
                if (match.Groups["Argument"].Success)
                {
                    if (int.TryParse(match.Groups["Argument"].Value, out var number))
                    {
                        if (number <= 0)
                            throw new ArgumentOutOfRangeException(nameof(number));

                        foreach (var item in _elementCollection.GetByType<RealmVehicle>().OrderBy(x => x.DistanceToSquared(_player)).Take(number))
                        {
                            if (allExceptMe && item == _player.Vehicle)
                                continue;

                            yield return item;
                        }
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
                else
                {
                    foreach (var item in _elementCollection.GetByType<RealmVehicle>().OrderBy(x => x.DistanceToSquared(_player)))
                    {
                        if (allExceptMe && item == _player.Vehicle)
                            continue;

                        yield return item;
                        break;
                    }
                }
                break;
            case SearchSwitch.R:
                if (int.TryParse(match.Groups["Argument"].Value, out var range))
                {
                    if (range < 0)
                        throw new ArgumentOutOfRangeException(nameof(range));

                    foreach (var item in _elementCollection.GetByType<RealmVehicle>().Where(x => x.DistanceToSquared(_player) < (range * range)))
                    {
                        if (allExceptMe && item == _player.Vehicle)
                            continue;

                        yield return item;
                    }
                }
                else
                {
                    throw new ArgumentException();
                }
                break;
            case SearchSwitch.A:
                foreach (var item in _elementCollection.GetByType<RealmVehicle>())
                {
                    if (allExceptMe && item == _player.Vehicle)
                        continue;

                    yield return item;
                }
                break;
            case SearchSwitch.C:
                if(_player.Vehicle != null)
                {
                    yield return _player.Vehicle;
                }
                break;
            default:
                throw new NotSupportedSwitchException(searchSwitch);
        }
    }

    private IEnumerable<RealmPlayer> SearchPlayersCore(string pattern, PlayerSearchOptions playerSearchOptions = default)
    {
        var match = Regexes.SpecialSearchSwitch().Match(pattern);

        if (!match.Success)
        {
            foreach (var player in _elementCollection.GetByType<RealmPlayer>())
            {
                if (playerSearchOptions.ignore != null && playerSearchOptions.ignore == player)
                    continue;

                if (playerSearchOptions.searchOptions.HasFlag(PlayerSearchOption.LoggedIn))
                {
                    if (!player.User.IsLoggedIn)
                        continue;
                }

                if (player.Name.Contains(pattern.ToLower(), StringComparison.CurrentCultureIgnoreCase))
                    yield return player;
            }

            yield break;
        }

        var search = match.Groups["Search"].Value;
        if (!Enum.TryParse<SearchSwitch>(search, true, out var searchSwitch))
            throw new UnknownSearchSwitchException(match.Groups["Character"].Value);

        var allExceptMe = char.IsUpper(search, 0);
        switch (searchSwitch)
        {
            case SearchSwitch.N:
                if (match.Groups["Argument"].Success)
                {
                    if(int.TryParse(match.Groups["Argument"].Value, out var number))
                    {
                        if (number <= 0)
                            throw new ArgumentOutOfRangeException(nameof(number));

                        foreach (var item in _elementCollection.GetByType<RealmPlayer>().OrderBy(x => x.DistanceToSquared(_player)).Take(number))
                        {
                            if (allExceptMe && item == _player)
                                continue;

                            yield return item;
                        }
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
                else
                {
                    foreach (var item in _elementCollection.GetByType<RealmPlayer>().OrderBy(x => x.DistanceToSquared(_player)))
                    {
                        if (allExceptMe && item == _player)
                            continue;

                        yield return item;
                        break;
                    }
                }
                break;
            case SearchSwitch.R:
                if (int.TryParse(match.Groups["Argument"].Value, out var range))
                {
                    if (range < 0)
                        throw new ArgumentOutOfRangeException(nameof(range));

                    foreach (var item in _elementCollection.GetByType<RealmPlayer>().Where(x => x.DistanceToSquared(_player) < (range * range)))
                    {
                        if (allExceptMe && item == _player)
                            continue;

                        yield return item;
                    }
                }
                else
                {
                    throw new ArgumentException();
                }
                break;
            case SearchSwitch.A:
                foreach (var item in _elementCollection.GetByType<RealmPlayer>())
                {
                    if (allExceptMe && item == _player)
                        continue;

                    yield return item;
                }
                break;
            default:
                throw new NotSupportedSwitchException(searchSwitch);
        }
    }
}
