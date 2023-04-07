namespace RealmCore.Server;

internal class EntityComponentsTransaction : IEntityComponentsTransaction
{
    private readonly byte _version;
    private readonly Entity _entity;
    private bool _state;
    private object _lock;

    public byte Version => _version;
    public Entity Entity => _entity;

    public EntityComponentsTransaction(byte version, Entity entity)
    {
        _version = version;
        _entity = entity;
        _state = false;
        _lock = new();
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_state)
                throw new InvalidOperationException("Transaction wasn't commited properly");
        }
    }

    public bool TryClose()
    {
        lock (_lock)
        {
            if (_state)
                return false;

            _state = true;
            return true;
        }
    }
}
