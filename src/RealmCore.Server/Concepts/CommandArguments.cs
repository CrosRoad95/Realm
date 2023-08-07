namespace RealmCore.Server.Concepts;

public class CommandArguments
{
    private readonly string[] _args;
    private int _index;

    public Span<string> Arguments => _args;

    public CommandArguments(string[] args)
    {
        _args = args;
    }

    public string ReadAllAsString()
    {
        return string.Join(", ", _args);
    }
    
    public string ReadWord()
    {
        var value = _args[_index++];
        return value;
    }

    public string ReadWordOrDefault(string defaultValue)
    {
        var value = _args[_index++];
        return value ?? defaultValue;
    }

    public int ReadInt()
    {
        var value = _args[_index++];
        return int.Parse(value);
    }

    public byte ReadByte()
    {
        var value = _args[_index++];
        return byte.Parse(value);
    }

    public ushort ReadUShort()
    {
        var value = _args[_index++];
        return ushort.Parse(value);
    }

    public uint ReadUInt()
    {
        var value = _args[_index++];
        return uint.Parse(value);
    }

    public decimal ReadDecimal()
    {
        var value = _args[_index++];
        return decimal.Parse(value);
    }
}
