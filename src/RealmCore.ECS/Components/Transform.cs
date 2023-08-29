using RealmCore.ECS.Attributes;
using System.Numerics;

namespace RealmCore.ECS.Components;

[ComponentUsage]
public sealed class Transform : Component
{
    private Vector3 _position;
    private Vector3 _rotation;
    private byte _interior;
    private ushort _dimension;
    public Vector3 Position
    {
        get => _position; set
        {
            if (_position != value)
            {
                _position = value;
                PositionChanged?.Invoke(this, value, true);
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
                RotationChanged?.Invoke(this, value, true);
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
                InteriorChanged?.Invoke(this, value, true);
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
                DimensionChanged?.Invoke(this, value, true);
            }
        }
    }

    public Vector3 Forward => new(MathF.Sin(-_rotation.Z * (float)(Math.PI / 180)), MathF.Cos(-_rotation.Z * (float)(Math.PI / 180)), 0);

    public event Action<Transform, Vector3, bool>? PositionChanged;
    public event Action<Transform, Vector3, bool>? RotationChanged;
    public event Action<Transform, byte, bool>? InteriorChanged;
    public event Action<Transform, ushort, bool>? DimensionChanged;

    public Transform() { }
    public Transform(Vector3 position, byte interior = 0, ushort dimension = 0)
    {
        _position = position;
        _interior = interior;
        _dimension = dimension;
    }
    public Transform(Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0)
    {
        _position = position;
        _rotation = rotation;
        _interior = interior;
        _dimension = dimension;
    }

    public void SetPosition(Vector3 position, bool sync)
    {
        if (_position != position)
        {
            _position = position;
            PositionChanged?.Invoke(this, position, sync);
        }
    }
    
    public void SetRotation(Vector3 rotation, bool sync)
    {
        if (_rotation != rotation)
        {
            _rotation = rotation;
            PositionChanged?.Invoke(this, rotation, sync);
        }
    }
    
    public void SetInterior(byte interior, bool sync)
    {
        if (_interior != interior)
        {
            _interior = interior;
            InteriorChanged?.Invoke(this, interior, sync);
        }
    }
    
    public void SetDimension(ushort dimension, bool sync)
    {
        if (_dimension != dimension)
        {
            _dimension = dimension;
            DimensionChanged?.Invoke(this, dimension, sync);
        }
    }

    //public void Bind(Element element)
    //{
    //    if (_isBound)
    //        throw new TransformAlreadyBoundException();
    //    _isBound = true;
    //    _element = element;
    //    element.Position = _position;
    //    if (element.ElementType != ElementType.Pickup)
    //        element.Rotation = _rotation;
    //    element.Interior = _interior;
    //    element.Dimension = _dimension;
    //    element.PositionChanged += HandlePositionChanged;
    //    element.RotationChanged += HandleRotationChanged;
    //    element.InteriorChanged += InteriorInteriorChanged;
    //    element.DimensionChanged += HandleDimensionChanged;
    //    element.Destroyed += HandleDestroyed;
    //}

    //private void HandlePositionChanged(Element _, ElementChangedEventArgs<Vector3> args)
    //{
    //    if (args.IsSync)
    //        Position = args.NewValue;
    //}

    //private void HandleRotationChanged(Element _, ElementChangedEventArgs<Vector3> args)
    //{
    //    if (args.IsSync)
    //        Rotation = args.NewValue;
    //}

    //private void InteriorInteriorChanged(Element _, ElementChangedEventArgs<byte> args)
    //{
    //    if (args.IsSync)
    //        Interior = args.NewValue;
    //}

    //private void HandleDimensionChanged(Element _, ElementChangedEventArgs<ushort> args)
    //{
    //    if (args.IsSync)
    //        Dimension = args.NewValue;
    //}
}
