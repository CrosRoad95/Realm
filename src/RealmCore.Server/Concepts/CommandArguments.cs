namespace RealmCore.Server.Concepts;

public class CommandArguments
{
    private readonly string[] _args;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUsersService _usersService;
    private int _index;

    public int Index => _index;
    public Span<string> Arguments => _args;

    public CommandArguments(string[] args, IServiceProvider serviceProvider)
    {
        _args = args;
        _serviceProvider = serviceProvider;
        _usersService = serviceProvider.GetRequiredService<IUsersService>();
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
                throw new CommandArgumentException(_index, "Argument jest pusty", null);
            return value;
        }
        else
        {
            throw new CommandArgumentException(_index, "Zbyt mało argumentów.", null);
        }
    }
    
    public bool TryReadArgument(out string argument)
    {
        if (_index >= 0 && _index < _args.Length)
        {
            var value = _args[_index++];
            if (string.IsNullOrWhiteSpace(value))
            {
                argument = string.Empty;
                return false;
            }
            argument = value;
        }
        argument = string.Empty;
        return false;
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
    
    public bool TryReadArgument<T>(out T argument)
    {
        if(TryReadArgument(out string arg))
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

    public string ReadWordOrDefault(string defaultValue)
    {
        var value = ReadArgument();
        return value ?? defaultValue;
    }

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

    public RealmPlayer ReadPlayer()
    {
        var name = ReadArgument();
        var users = _usersService.SearchPlayersByName(name).ToList();
        if (users.Count == 1)
            return users[0];
        if(users.Count > 0)
            throw new CommandArgumentException(_index, "Znaleziono więcej niż 1 gracza o takiej nazwie", name);
        throw new CommandArgumentException(_index, "Gracz o takiej nazwie nie został znaleziony", name);
    }
    
    public bool TryReadPlayerPlayer(out RealmPlayer? player)
    {
        if(TryReadArgument(out string argument))
        {
            var name = ReadArgument();
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
