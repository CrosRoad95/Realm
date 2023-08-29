namespace RealmCore.Server.IdGenerators;

public class MapIdGenerator : IElementIdGenerator
{
    private readonly uint _start;
    private readonly uint _stop;
    private readonly object _lock = new();
    private uint _idCounter;

    public MapIdGenerator(uint start, uint stop)
    {
        _idCounter = start;
        _start = start;
        _stop = stop;
    }

    public uint GetId()
    {
        lock (_lock)
        {
            _idCounter++;
            if (_idCounter > _stop)
                _idCounter = _start;

            return _idCounter;
        }
    }
}
