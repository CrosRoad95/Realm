namespace RealmCore.Server.Modules.Search;

public interface IElementSearchService
{
    IEnumerable<RealmPlayer> SearchPlayers(string pattern, PlayerSearchOption searchOptions = PlayerSearchOption.All, RealmPlayer? ignore = null);
    IEnumerable<RealmVehicle> SearchVehicles(string pattern, VehicleSearchOption searchOptions = VehicleSearchOption.All, RealmVehicle? ignore = null);
}

public sealed class ElementSearchService : IElementSearchService
{
    private readonly IElementCollection _elementCollection;
    private readonly RealmPlayer _player;

    public ElementSearchService(PlayerContext playerContext, IElementCollection elementCollection)
    {
        _player = playerContext.Player;
        _elementCollection = elementCollection;
    }

    public IEnumerable<RealmVehicle> SearchVehicles(string pattern, VehicleSearchOption searchOptions = VehicleSearchOption.All, RealmVehicle? ignore = null)
    {
        foreach (var vehicle in SearchVehiclesInner(pattern, searchOptions))
        {
            if (ignore != null && ignore == vehicle)
                continue;
            yield return vehicle;
        }
    }

    public IEnumerable<RealmVehicle> SearchVehiclesInner(string pattern, VehicleSearchOption searchOptions = VehicleSearchOption.All)
    {
        var match = Regexes.SpecialSearchSwitch().Match(pattern);
        if (match.Success)
        {
            var search = match.Groups["Search"].Value;
            if (Enum.TryParse<SearchSwitch>(search, true, out var searchSwitch))
            {
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
            else
            {
                throw new UnknownSearchSwitchException(match.Groups["Character"].Value);
            }
        }
    }

    public IEnumerable<RealmPlayer> SearchPlayers(string pattern, PlayerSearchOption searchOptions = PlayerSearchOption.All, RealmPlayer? ignore = null)
    {
        var match = Regexes.SpecialSearchSwitch().Match(pattern);
        if (match.Success)
        {
            var search = match.Groups["Search"].Value;
            if (Enum.TryParse<SearchSwitch>(search, true, out var searchSwitch))
            {
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
            else
            {
                throw new UnknownSearchSwitchException(match.Groups["Character"].Value);
            }
        }

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

}
