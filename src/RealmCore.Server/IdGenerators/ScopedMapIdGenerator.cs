namespace RealmCore.Server.IdGenerators;

public sealed class ScopedMapIdGenerator : IElementIdGenerator
{
    private readonly uint _start;
    private readonly uint _stop;
    private readonly object _lock = new();
    private uint _idCounter;

    public ScopedMapIdGenerator()
    {
        _idCounter = IdGeneratorConstants.PlayerIdStart;
        _start = IdGeneratorConstants.PlayerIdStart;
        _stop = IdGeneratorConstants.PlayerIdStop;
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
