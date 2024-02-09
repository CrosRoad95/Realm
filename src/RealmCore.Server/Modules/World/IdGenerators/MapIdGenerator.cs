namespace RealmCore.Server.Modules.World.IdGenerators;

public sealed class MapIdGenerator : IElementIdGenerator
{
    private readonly uint _start;
    private readonly uint _stop;
    private readonly object _lock = new();
    private uint _idCounter;

    public MapIdGenerator()
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
