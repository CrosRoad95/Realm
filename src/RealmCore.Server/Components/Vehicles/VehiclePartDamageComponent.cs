using RealmCore.Persistance.Data;

namespace RealmCore.Server.Components.Vehicles;

public class VehiclePartDamageComponent : Component
{
    /// <summary>
    /// Triggered when part damage drop to zero.
    /// </summary>
    public event Action<VehiclePartDamageComponent, short>? PartDestroyed;
    private readonly ConcurrentDictionary<short, float> _partDamages = new();

    public ICollection<short> Parts
    {
        get
        {
            ThrowIfDisposed();
            return _partDamages.Keys;
        }
    }

    public VehiclePartDamageComponent() { }

    internal VehiclePartDamageComponent(ICollection<VehiclePartDamageData> vehiclePartDamages)
    {
        foreach (var item in vehiclePartDamages)
            _partDamages.TryAdd(item.PartId, item.State);
    }

    public void AddPart(short partId, float state)
    {
        ThrowIfDisposed();

        if (state < 0)
            throw new ArgumentOutOfRangeException(nameof(state));
        if (!_partDamages.TryAdd(partId, state))
            throw new ArgumentException("Part id already added");

        if (state == 0)
            Update(partId);
    }

    public bool RemovePart(short partId)
    {
        ThrowIfDisposed();

        if (_partDamages.TryRemove(partId, out _))
        {
            PartDestroyed?.Invoke(this, partId);
            return true;
        }
        return false;
    }

    public void Modify(short partId, float difference)
    {
        ThrowIfDisposed();

        if (_partDamages.TryGetValue(partId, out var state))
        {
            var newState = state + difference;
            if (newState <= 0)
                newState = 0;

            if (_partDamages.TryUpdate(partId, newState, state) && newState == 0)
                Update(partId);
        }
    }

    public float? Get(short partId)
    {
        ThrowIfDisposed();

        if (_partDamages.TryGetValue(partId, out var state))
            return state;
        return null;
    }

    private void Update(short partId)
    {
        PartDestroyed?.Invoke(this, partId);
    }
}
