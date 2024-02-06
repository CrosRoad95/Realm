namespace RealmCore.Server.Concepts;

public class CommandArguments
{
    private readonly string[] _args;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUsersService _usersService;
    private int _index;

    public int Index => _index;

    public CommandArguments(string[] args, IServiceProvider serviceProvider)
    {
        _args = args;
        _serviceProvider = serviceProvider;
        _usersService = _serviceProvider.GetRequiredService<IUsersService>();
    }

    internal void Left() // For test purpose
    {
        _index--;
        if (_index < 0)
            throw new InvalidOperationException();
    }
    
    public void End()
    {
        if(_index < _args.Length)
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
            if(string.IsNullOrWhiteSpace(value))
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
        catch(Exception)
        {
            throw new CommandArgumentException(_index, "Podany argument jest nie poprawny.", null);
        }
    }
    
    public bool TryReadArgument<T>(out T? argument)
    {
        if(TryReadArgument(out string? arg) && arg != null)
        {
            try
            {
                argument = (T)Convert.ChangeType(arg, typeof(T));
                return true;
            }
            catch(Exception)
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

    public RealmPlayer ReadPlayer(bool loggedIn = true)
    {
        var name = ReadArgument();
        var users = _usersService.SearchPlayersByName(name, loggedIn).ToList();
        if (users.Count == 1)
            return users[0];
        if(users.Count > 0)
            throw new CommandArgumentException(_index, "Znaleziono więcej niż 1 gracza o takiej nazwie", name);
        throw new CommandArgumentException(_index, "Gracz o takiej nazwie nie został znaleziony", name);
    }
    
    public bool TryReadPlayerPlayer(out RealmPlayer? player)
    {
        if(TryReadArgument(out string? name) && name != null)
        {
            var players = _usersService.SearchPlayersByName(name).ToList();
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

    public IEnumerable<RealmPlayer> ReadPlayers()
    {
        var name = ReadArgument();
        var users = _usersService.SearchPlayersByName(name).ToList();
        if (users.Count == 0)
            throw new CommandArgumentException(_index, "Nie znaleziono żadnego gracza o takiej nazwie", name);
        return users;
    }

    public TEnum? ReadEnum<TEnum>() where TEnum : struct
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
}
