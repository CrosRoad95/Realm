﻿namespace RealmCore.Server.Components;

public sealed class Transform
{
    private Vector3 _position;
    private Vector3 _rotation;
    private byte _interior;
    private ushort _dimension;
    private Element? _element = null;
    private bool _isBound = false;

    public Entity Entity { get; private set; }

    public Vector3 Position
    {
        get => _position; set
        {
            if (_position != value)
            {
                _position = value;
                if (_element != null)
                    _element.Position = value;
                PositionChanged?.Invoke(this);
            }
        }
    }

    public Vector3 Rotation
    {
        get => _rotation; set
        {
            if (_rotation != value)
            {
                _rotation = value;
                if (_element != null)
                    _element.Rotation = value;
                RotationChanged?.Invoke(this);
            }
        }
    }

    public byte Interior
    {
        get => _interior; set
        {
            if (_interior != value)
            {
                _interior = value;
                if (_element != null)
                    _element.Interior = value;
                InteriorChanged?.Invoke(this);
            }
        }
    }

    public ushort Dimension
    {
        get => _dimension; set
        {
            if (_dimension != value)
            {
                _dimension = value;
                if (_element != null)
                    _element.Dimension = value;
                DimensionChanged?.Invoke(this);
            }
        }
    }

    public Vector3 Forward => new(MathF.Sin(-_rotation.Z * (float)(Math.PI / 180)), MathF.Cos(-_rotation.Z * (float)(Math.PI / 180)), 0);

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
        if (_isBound)
            throw new TransformAlreadyBoundException();
        _isBound = true;
        _element = element;
        element.Position = _position;
        if (element.ElementType != ElementType.Pickup)
            element.Rotation = _rotation;
        element.Interior = _interior;
        element.Dimension = _dimension;
        element.PositionChanged += HandlePositionChanged;
        element.RotationChanged += HandleRotationChanged;
        element.InteriorChanged += InteriorInteriorChanged;
        element.DimensionChanged += HandleDimensionChanged;
        element.Destroyed += HandleDestroyed;
    }

    private void HandlePositionChanged(Element _, ElementChangedEventArgs<Vector3> args)
    {
        if (args.IsSync)
            Position = args.NewValue;
    }

    private void HandleRotationChanged(Element _, ElementChangedEventArgs<Vector3> args)
    {
        if (args.IsSync)
            Rotation = args.NewValue;
    }

    private void InteriorInteriorChanged(Element _, ElementChangedEventArgs<byte> args)
    {
        if (args.IsSync)
            Interior = args.NewValue;
    }

    private void HandleDimensionChanged(Element _, ElementChangedEventArgs<ushort> args)
    {
        if (args.IsSync)
            Dimension = args.NewValue;
    }

    private void HandleDestroyed(Element element)
    {
        element.PositionChanged -= HandlePositionChanged;
        element.RotationChanged -= HandleRotationChanged;
        element.InteriorChanged -= InteriorInteriorChanged;
        element.DimensionChanged -= HandleDimensionChanged;
    }

    public TransformAndMotion GetTransformAndMotion()
    {
        return new TransformAndMotion
        {
            Position = _position,
            Rotation = _rotation,
            Interior = _interior,
            Dimension = _dimension,
        };
    }
}