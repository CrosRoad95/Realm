﻿using SlipeServer.Server.Elements.Events;

namespace Realm.Domain.Components.Common;

public sealed class Transform
{
    private Vector3 _position;
    private Vector3 _rotation;
    private byte _interior;
    private ushort _dimension;
    private bool _isBounded = false;

    public Entity Entity { get; private set; }

    public Vector3 Position
    {
        get => _position; set
        {
            if (_position != value)
            {
                PositionChanged?.Invoke(this);
                _position = value;
            }
        }
    }

    public Vector3 Rotation
    {
        get => _rotation; set
        {
            if (_rotation != value)
            {
                RotationChanged?.Invoke(this);
                _rotation = value;
            }
        }
    }

    public byte Interior
    {
        get => _interior; set
        {
            if (_interior != value)
            {
                InteriorChanged?.Invoke(this);
                _interior = value;
            }
        }
    }

    public ushort Dimension
    {
        get => _dimension; set
        {
            if (_dimension != value)
            {
                DimensionChanged?.Invoke(this);
                _dimension = value;
            }
        }
    }

    public event Action<Transform>? PositionChanged;
    public event Action<Transform>? RotationChanged;
    public event Action<Transform>? InteriorChanged;
    public event Action<Transform>? DimensionChanged;
    public Transform(Entity entity)
    {
        Entity = entity;
    }

    public void Bind(Element element)
    {
        if (_isBounded)
            throw new Exception("Transform already bounded");
        _isBounded = true;
        element.PositionChanged += HandlePositionChanged;
        element.RotationChanged += HandleRotationChanged;
        element.InteriorChanged += InteriorInteriorChanged;
        element.DimensionChanged += HandleDimensionChanged;
        element.Destroyed += HandleDestroyed;
        element.Position = Position;
        element.Rotation = Rotation;
        element.Interior = Interior;
        element.Dimension = Dimension;
    }

    private void HandlePositionChanged(Element _, ElementChangedEventArgs<Vector3> args)
    {
        Position = args.NewValue;
    }

    private void HandleRotationChanged(Element _, ElementChangedEventArgs<Vector3> args)
    {
        Rotation = args.NewValue;
    }

    private void InteriorInteriorChanged(Element _, ElementChangedEventArgs<byte> args)
    {
        Interior = args.NewValue;
    }

    private void HandleDimensionChanged(Element _, ElementChangedEventArgs<ushort> args)
    {
        Dimension = args.NewValue;
    }

    private void HandleDestroyed(Element element)
    {
        element.PositionChanged -= HandlePositionChanged;
        element.RotationChanged -= HandleRotationChanged;
        element.InteriorChanged -= InteriorInteriorChanged;
        element.DimensionChanged -= HandleDimensionChanged;
    }
}