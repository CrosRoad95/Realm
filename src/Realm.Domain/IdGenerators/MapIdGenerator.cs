using SlipeServer.Server.Elements.IdGeneration;

namespace Realm.Domain.IdGenerators;

public class MapIdGenerator : IElementIdGenerator
{
    private readonly uint _start;
    private readonly uint _stop;
    private uint _idCounter;

    public MapIdGenerator(uint start, uint stop)
    {
        _idCounter = start;
        _start = start;
        _stop = stop;
    }

    public uint GetId()
    {
        _idCounter++;
        if (_idCounter > _stop)
            _idCounter = _start;

        return _idCounter;
    }
}
