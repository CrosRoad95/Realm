using RealmCore.Server.Structs;

namespace RealmCore.Server.Contexts;

public class VehicleAccess : IDisposable
{
    private readonly List<VehiclePlayerAccess> _vehiclePlayerAccesses;
    private readonly object _lock = new();
    private Component? _component;
    protected bool _disposed = false;

    public VehicleAccess(IEnumerable<VehicleUserAccessData> vehicleUserAccessData, Component? component = null)
    {
        _vehiclePlayerAccesses = vehicleUserAccessData.Select(x => new VehiclePlayerAccess
        {
            id = x.Id,
            userId = x.User.Id,
            accessType = x.AccessType,
            customValue = x.CustomValue
        }).ToList();

        _component = component;
        if(_component != null)
        {
            _component.Disposed += HandleComponentDisposed;
        }
    }

    private void HandleComponentDisposed(Component component)
    {
        component.Disposed -= HandleComponentDisposed;
        _component = null;
        _disposed = true;
    }

    public IReadOnlyList<VehiclePlayerAccess> PlayerAccesses
    {
        get
        {
            ThrowIfDisposed();
            lock (_lock)
                return new List<VehiclePlayerAccess>(_vehiclePlayerAccesses);
        }
    }

    public IReadOnlyList<VehiclePlayerAccess> Owners
    {
        get
        {
            ThrowIfDisposed();
            lock (_lock)
                return new List<VehiclePlayerAccess>(_vehiclePlayerAccesses.Where(x => x.accessType == 0));
        }
    }

    public bool TryGetAccess(Entity entity, out VehiclePlayerAccess vehicleAccess)
    {
        ThrowIfDisposed();

        var tag = entity.GetRequiredComponent<TagComponent>();
        if (tag is not PlayerTagComponent)
            throw new InvalidOperationException();

        var userId = entity.GetRequiredComponent<UserComponent>().Id;
        lock (_lock)
        {
            var index = _vehiclePlayerAccesses.FindIndex(x => x.userId == userId);
            if (index >= 0)
            {
                vehicleAccess = _vehiclePlayerAccesses[index];
                return true;
            }
        }
        vehicleAccess = default;
        return false;
    }

    public bool HasAccess(Entity entity)
    {
        ThrowIfDisposed();
        if (TryGetAccess(entity, out VehiclePlayerAccess vehiclePlayerAccess))
        {
            return true;
        }
        return false;
    }

    public VehiclePlayerAccess AddAccess(Entity entity, byte accessType, string? customValue = null)
    {
        ThrowIfDisposed();

        var tag = entity.GetRequiredComponent<TagComponent>();
        if (tag is not PlayerTagComponent)
            throw new InvalidOperationException();

        if (TryGetAccess(entity, out var _))
            throw new EntityAccessDefinedException();

        lock (_lock)
        {
            _vehiclePlayerAccesses.Add(new VehiclePlayerAccess
            {
                userId = entity.GetRequiredComponent<UserComponent>().Id,
                accessType = accessType,
                customValue = customValue
            });
            return _vehiclePlayerAccesses.Last();
        }
    }

    public VehiclePlayerAccess AddAsOwner(Entity entity, string? customValue = null)
    {
            ThrowIfDisposed();
        ThrowIfDisposed();
        return AddAccess(entity, 0, customValue);
    }

    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose()
    {
        if(_component != null)
        {
            _component.Disposed -= HandleComponentDisposed;
            _component = null;
        }
        _disposed = true;
    }
}
