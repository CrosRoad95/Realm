namespace RealmCore.Server.Concepts;

public class CommandArguments
{
    private readonly string[] _args;
    private readonly IUsersService _usersService;
    private int _index;
    public Span<string> Arguments => _args;

    public CommandArguments(string[] args, IUsersService usersService)
    {
        _args = args;
        _usersService = usersService;
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

    public Entity ReadPlayerEntity()
    {
        var name = ReadArgument();
        var users = _usersService.SearchPlayersByName(name).ToList();
        if (users.Count == 1)
            return users[0];
        if(users.Count > 0)
            throw new CommandArgumentException(_index, "Znaleziono więcej niż 1 gracza o takiej nazwie", name);
        throw new CommandArgumentException(_index, "Gracz o takiej nazwie nie został znaleziony", name);
    }

    public IEnumerable<Entity> ReadPlayerEntities()
    {
        var name = ReadArgument();
        var users = _usersService.SearchPlayersByName(name).ToList();
        if (users.Count == 0)
            throw new CommandArgumentException(_index, "Nie znaleziono żadnego gracza o takiej nazwie", name);
        return users;
    }
}
