using RealmCore.Server.Modules.Search;

namespace RealmCore.Server.Modules.Commands;

public class CommandArguments
{
    private readonly string[] _args;
    private readonly RealmPlayer _player;
    private readonly IPlayersService _playersService;
    private readonly IElementSearchService _searchService;
    private int _index;

    public int Index => _index;

    public CommandArguments(RealmPlayer player, IPlayersService playersService, IElementSearchService searchService, string[] args)
    {
        _args = args;
        _player = player;
        _playersService = playersService;
        _searchService = searchService;
    }

    internal void Left() // For test purpose
    {
        _index--;
        if (_index < 0)
            throw new InvalidOperationException();
    }

    public void End()
    {
        if (_index < _args.Length)
            throw new CommandArgumentException(_index, "Zbyt dużo argumentów.", null);
    }

    public string ReadAllAsString()
    {
        return string.Join(", ", _args.Skip(_index));
    }

    public string ReadArgument()
    {
        if (_index >= 0 && _index < _args.Length)
        {
            var value = _args[_index++];
            if (string.IsNullOrWhiteSpace(value))
                throw new CommandArgumentException(_index, "Argument jest pusty.", null);
            return value;
        }
        else
        {
            throw new CommandArgumentException(_index, "Zbyt mało argumentów.", null);
        }
    }

    private bool TryReadArgument(out string? argument)
    {
        if (_index >= 0 && _index < _args.Length)
        {
            var value = _args[_index++];
            if (string.IsNullOrWhiteSpace(value))
            {
                argument = default;
                return false;
            }
            argument = value;
            return true;
        }
        else
        {
            argument = default;
            return false;
        }
    }

    public T ReadArgument<T>()
    {
        var argument = ReadArgument();
        try
        {
            return (T)Convert.ChangeType(argument, typeof(T));
        }
        catch (Exception)
        {
            throw new CommandArgumentException(_index, "Podany argument jest nie poprawny.", null);
        }
    }

    public bool TryReadArgument<T>(out T? argument)
    {
        if (TryReadArgument(out string? arg) && arg != null)
        {
            try
            {
                argument = (T)Convert.ChangeType(arg, typeof(T));
                return true;
            }
            catch (Exception)
            {
                throw new CommandArgumentException(_index, "Podany argument jest nie poprawny.", null);
            }
        }
        argument = default;
        return false;
    }

    public string? ReadWordOrDefault()
    {
        if (TryReadArgument(out var argument))
            return argument;
        return null;
    }

    public string ReadWordOrDefault(string defaultValue)
    {
        if (TryReadArgument(out var argument) && argument != null)
            return argument;
        return defaultValue;
    }

    public string ReadWord() => ReadArgument();

    public int ReadInt()
    {
        var value = ReadArgument();
        if (int.TryParse(value, out var value2))
            return value2;
        throw new CommandArgumentException(_index, "Liczba jest poza zakresem", value);
    }

    public byte ReadByte()
    {
        var value = ReadArgument();
        if (byte.TryParse(value, out var value2))
            return value2;
        throw new CommandArgumentException(_index, "Liczba jest poza zakresem", value);
    }

    public ushort ReadUShort()
    {
        var value = ReadArgument();
        if (ushort.TryParse(value, out var value2))
            return value2;
        throw new CommandArgumentException(_index, "Liczba jest poza zakresem", value);
    }

    public uint ReadUInt()
    {
        var value = ReadArgument();
        return uint.Parse(value);
    }

    public decimal ReadDecimal()
    {
        var value = ReadArgument();
        if (decimal.TryParse(value, out var value2))
            return value2;
        throw new CommandArgumentException(_index, "Liczba jest poza zakresem", value);
    }

    public virtual RealmPlayer ReadPlayer(PlayerSearchOption searchOption = PlayerSearchOption.All, RealmPlayer? ignore = null)
    {
        var name = ReadArgument();
        var users = _searchService.SearchPlayers(name, searchOption, ignore).ToList();
        if (users.Count == 1)
            return users[0];
        if (users.Count > 0)
            throw new CommandArgumentException(_index, "Znaleziono więcej niż 1 gracza o takiej nazwie", name);
        throw new CommandArgumentException(_index, "Gracz o takiej nazwie nie został znaleziony", name);
    }

    public virtual bool TryReadPlayerPlayer(out RealmPlayer? player, PlayerSearchOption searchOption = PlayerSearchOption.All, RealmPlayer? ignore = null)
    {
        if (TryReadArgument(out string? name) && name != null)
        {
            var players = _searchService.SearchPlayers(name, searchOption, ignore).ToList();
            if (players.Count == 1)
            {
                player = players[0];
                return true;
            }
            if (players.Count > 0)
                throw new CommandArgumentException(_index, "Znaleziono więcej niż 1 gracza o takiej nazwie", name);
            throw new CommandArgumentException(_index, "Gracz o takiej nazwie nie został znaleziony", name);
        }
        player = null;
        return false;
    }

    public virtual IEnumerable<RealmPlayer> SearchPlayers(string? pattern = null, PlayerSearchOption searchOption = PlayerSearchOption.All, RealmPlayer? ignore = null)
    {
        pattern ??= ReadArgument();
        var players = _searchService.SearchPlayers(pattern).ToList();
        if (players.Count == 0 && !searchOption.HasFlag(PlayerSearchOption.AllowEmpty))
            throw new CommandArgumentException(_index, "Nie znaleziono żadnego gracza.", pattern);
        return players;
    }

    public virtual IEnumerable<RealmVehicle> SearchVehicles(string? pattern = null, PlayerSearchOption searchOption = PlayerSearchOption.All, RealmPlayer? ignore = null)
    {
        pattern ??= ReadArgument();
        var vehicles = _searchService.SearchVehicles(pattern).ToList();
        if (vehicles.Count == 0)
            throw new CommandArgumentException(_index, "Nie znaleziono żadnego pojazdu.", pattern);
        return vehicles;
    }

    public TEnum ReadEnum<TEnum>() where TEnum : struct
    {
        var str = ReadArgument();
        if (int.TryParse(str, out int intValue))
        {
            if (Enum.IsDefined(typeof(TEnum), intValue))
                return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
        }
        else
        {
            if (Enum.TryParse<TEnum>(str, out var value))
            {
                return value;
            }
        }
        throw new CommandArgumentException(_index, null, str);
    }

    public TEnum ReadEnumOrDefault<TEnum>(TEnum defaultValue = default) where TEnum : struct
    {
        var str = ReadArgument();
        if (int.TryParse(str, out int intValue))
        {
            if (Enum.IsDefined(typeof(TEnum), intValue))
                return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
        }
        else
        {
            if (Enum.TryParse<TEnum>(str, out var value))
            {
                return value;
            }
        }
        return defaultValue;
    }

    public bool TryReadEnum<TEnum>(out TEnum outValue) where TEnum : struct
    {
        if (!TryReadArgument(out var str))
        {
            outValue = default;
            return false;
        }

        if (int.TryParse(str, out int intValue))
        {
            if (Enum.IsDefined(typeof(TEnum), intValue))
            {
                outValue = (TEnum)Enum.ToObject(typeof(TEnum), intValue);
                return true;
            }
        }
        else
        {
            if (Enum.TryParse<TEnum>(str, out var value))
            {
                outValue = value;
                return true;
            }
        }

        outValue = default;
        return false;
    }
}
