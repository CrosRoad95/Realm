﻿using Realm.Domain.Components.Elements;
using SlipeServer.Server.Elements.Events;

namespace Realm.Domain.Components.Vehicles;

[Serializable]
public class VehicleFuelComponent : Component
{
    public event Action<VehicleFuelComponent>? FuelRanOut;

    private Vector3 _lastPosition;
    private float _amount = 0;
    private float _maxCapacity = 0;
    private float _fuelConsumptionPerOneKm;
    private float _minimumDistanceThreshold;
    private string _fuelType;

    public float MinimumDistanceThreshold
    {
        get => _minimumDistanceThreshold;
        set
        {
            if (value < 0.0f) value = 0.0f;
            _minimumDistanceThreshold = value;
        }
    }

    public float FuelConsumptionPerOneKm
    {
        get => _fuelConsumptionPerOneKm;
        set
        {
            if (value < 0.0f) value = 0.0f;
            _fuelConsumptionPerOneKm = value;
        }
    }

    public float Amount
    {
        get => _amount;
        set
        {
            if (value < 0.0f) value = 0.0f;
            if (value >= MaxCapacity) value = MaxCapacity;
            _amount = value;
            Update(true);
        }
    }

    public float MaxCapacity
    {
        get => _maxCapacity;
        set
        {
            if (value < 0.0f)
                _maxCapacity = 0;
            else
                _maxCapacity = value;
            if (_maxCapacity < _amount)
                _amount = _maxCapacity;
            Update(true);
        }
    }

    public string FuelType
    {
        get => _fuelType;
        set => _fuelType = value;
    }

    public VehicleFuelComponent(double initialAmount, double maxCapacity, double fuelConsumptionPerOneKm, double minimumDistanceThreshold, string fuelType)
    {
        if (initialAmount < 0) throw new ArgumentOutOfRangeException(nameof(initialAmount));
        if (minimumDistanceThreshold < 0) throw new ArgumentOutOfRangeException(nameof(minimumDistanceThreshold));
        if (fuelConsumptionPerOneKm < 0) throw new ArgumentOutOfRangeException(nameof(fuelConsumptionPerOneKm));
        if (initialAmount >= maxCapacity) initialAmount = maxCapacity;

        _amount = (float)initialAmount;
        _maxCapacity = (float)maxCapacity;
        _fuelConsumptionPerOneKm = (float)fuelConsumptionPerOneKm;
        _minimumDistanceThreshold = (float)minimumDistanceThreshold;
        _fuelType = fuelType;
    }

    private void RegisterEvents()
    {
        var vehicle = Entity.InternalGetRequiredComponent<VehicleElementComponent>().Vehicle;
        vehicle.PositionChanged += HandlePositionChanged;
    }

    private void UnregisterEvents()
    {
        var vehicle = Entity.InternalGetRequiredComponent<VehicleElementComponent>().Vehicle;
        vehicle.PositionChanged -= HandlePositionChanged;
    }

    private void HandlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        if (args.IsSync)
            Update();
    }

    private void Update(bool forceUpdate = false)
    {
        var vehicle = Entity.InternalGetRequiredComponent<VehicleElementComponent>().Vehicle;
        if (!vehicle.IsEngineOn && !forceUpdate)
        {
            _lastPosition = vehicle.Position;
            return;
        }
        if ((!vehicle.IsEngineOn || vehicle.IsFrozen) && !forceUpdate)
            return;

        var traveledDistance = vehicle.Position - _lastPosition;
        if (_minimumDistanceThreshold > traveledDistance.Length() && !forceUpdate)
            return;
        _lastPosition = vehicle.Position;
        var consumedFuel = _fuelConsumptionPerOneKm / 1000.0f * traveledDistance.Length();
        _amount -= consumedFuel;
        if (_amount <= 0)
        {
            _amount = 0;
            vehicle.IsEngineOn = false;
            FuelRanOut?.Invoke(this);
        }
    }
}