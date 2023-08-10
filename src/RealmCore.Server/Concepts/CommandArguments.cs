using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;

namespace RealmCore.Server.Concepts;

public class CommandArguments
{
    private readonly string[] _args;
    private readonly IUsersService _usersService;
    private int _index;
    private int _expectedArguments = -1;
    public Span<string> Arguments => _args;

    public CommandArguments(string[] args, IUsersService usersService)
    {
        _args = args;
        _usersService = usersService;
    }

    public string ReadAllAsString()
    {
        return string.Join(", ", _args);
    }

    public string ReadWord()
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

    public string ReadWordOrDefault(string defaultValue)
    {
        var value = ReadWord();
        return value ?? defaultValue;
    }

    public int ReadInt()
    {
        var value = ReadWord();
        if (int.TryParse(value, out var value2))
            return value2;
        throw new CommandArgumentException(_index, "Liczba jest poza zakresem", value);
    }

    public byte ReadByte()
    {
        var value = ReadWord();
        if (byte.TryParse(value, out var value2))
            return value2;
        throw new CommandArgumentException(_index, "Liczba jest poza zakresem", value);
    }

    public ushort ReadUShort()
    {
        var value = ReadWord();
        if (ushort.TryParse(value, out var value2))
            return value2;
        throw new CommandArgumentException(_index, "Liczba jest poza zakresem", value);
    }

    public uint ReadUInt()
    {
        var value = ReadWord();
        return uint.Parse(value);
    }

    public decimal ReadDecimal()
    {
        var value = ReadWord();
        if (decimal.TryParse(value, out var value2))
            return value2;
        throw new CommandArgumentException(_index, "Liczba jest poza zakresem", value);
    }

    public Entity? ReadPlayerEntity()
    {
        var name = ReadWord();
        var users = _usersService.SearchPlayersByName(name).ToList();
        if (users.Count == 1)
            return users[0];
        if(users.Count > 0)
            throw new CommandArgumentException(_index, "Znaleziono więcej niż 1 gracza o takiej nazwie", name);
        throw new CommandArgumentException(_index, "Gracz o takiej nazwie nie został znaleziony", name);
    }

    public IEnumerable<Entity> ReadPlayerEntities()
    {
        var name = ReadWord();
        var users = _usersService.SearchPlayersByName(name).ToList();
        if (users.Count == 0)
            throw new CommandArgumentException(_index, "Nie znaleziono żadnego gracza o takiej nazwie", name);
        return users;
    }
}
