﻿namespace RealmCore.Server.Modules.Vehicles;

public sealed class VehiclePartDamageFeature : IVehicleFeature, IUsesVehiclePersistentData
{
    private readonly object _lock = new();
    private ICollection<VehiclePartDamageData> _vehiclePartDamages = [];
    public event Action<VehiclePartDamageFeature, short>? PartRemoved;
    public event Action? VersionIncreased;

    public short[] Parts
    {
        get
        {
            lock (_lock)
            {
                return [.. _vehiclePartDamages.Select(x => x.PartId)];
            }
        }
    }

    public RealmVehicle Vehicle { get; init; }

    public VehiclePartDamageFeature(VehicleContext vehicleContext)
    {
        Vehicle = vehicleContext.Vehicle;
    }

    public bool TryAddPart(short partId, float state)
    {
        lock (_lock)
        {
            if (state <= 0)
                throw new ArgumentOutOfRangeException(nameof(state));
            var exists = _vehiclePartDamages.FirstOrDefault(x => x.PartId == partId);
            if (exists != null)
                return false;

            _vehiclePartDamages.Add(new VehiclePartDamageData
            {
                PartId = partId,
                State = state
            });
        }

        VersionIncreased?.Invoke();

        return true;
    }

    public bool TryRemovePart(short partId)
    {
        bool removed = false;
        lock (_lock)
        {
            var vehiclePartDamage = _vehiclePartDamages.FirstOrDefault(x => x.PartId == partId);
            if (vehiclePartDamage == null)
                return false;

            removed = _vehiclePartDamages.Remove(vehiclePartDamage);
        }

        if(removed)
        {
            VersionIncreased?.Invoke();
            PartRemoved?.Invoke(this, partId);
        }

        return removed;
    }

    public bool TryModify(short partId, float difference)
    {
        float newState = 0;
        lock (_lock)
        {
            var vehiclePartDamage = _vehiclePartDamages.FirstOrDefault(x => x.PartId == partId);
            if (vehiclePartDamage == null)
                return false;

            vehiclePartDamage.State += difference;
            if (vehiclePartDamage.State <= 0)
                vehiclePartDamage.State = 0;
            newState = vehiclePartDamage.State;
        }

        if (newState == 0)
        {
            PartRemoved?.Invoke(this, partId);
            VersionIncreased?.Invoke();
            return true;
        }

        return false;
    }

    public bool HasPart(short partId)
    {
        lock (_lock)
        {
            return _vehiclePartDamages.Any(x => x.PartId == partId);
        }
    }

    public bool TryGetState(short partId, out float state)
    {
        lock (_lock)
        {
            var vehiclePartDamage = _vehiclePartDamages.FirstOrDefault(x => x.PartId == partId);
            if (vehiclePartDamage == null)
            {
                state = 0;
                return false;
            }
            state = vehiclePartDamage.State;
            return true;
        }
    }

    public float GetState(short partId)
    {
        lock (_lock)
        {
            var vehiclePartDamage = _vehiclePartDamages.First(x => x.PartId == partId);
            return vehiclePartDamage.State;
        }
    }

    public void Loaded(VehicleData vehicleData, bool preserveData = false)
    {
        _vehiclePartDamages = vehicleData.PartDamages;
    }

    public void Unloaded()
    {

    }
}
