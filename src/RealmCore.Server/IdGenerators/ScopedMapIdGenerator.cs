namespace RealmCore.Server.IdGenerators;

public sealed class ScopedMapIdGenerator : IElementIdGenerator
{
    private readonly uint _start;
    private readonly uint _stop;
    private readonly object _lock = new();
    private uint _idCounter;

    public ScopedMapIdGenerator()
    {
        _idCounter = IdGeneratorConstants.MapIdStart;
        _start = IdGeneratorConstants.MapIdStart;
        _stop = IdGeneratorConstants.MapIdStop;
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
