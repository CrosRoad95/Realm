namespace RealmCore.Server.Components.Elements.Abstractions;

public abstract class ElementComponent : Component
{
    abstract internal Element Element { get; }
    private Player? Player { get; set; }
    private bool _isPerPlayer = false;

    internal bool BaseLoaded { get; set; } = false;
    private Transform? _transform;

    public Vector3 Velocity
    {
        get
        {
            ThrowIfDisposed();
            return Element.Velocity;
        }
        set
        {
            ThrowIfDisposed();
            Element.Velocity = value;
        }
    }

    public Vector3 TurnVelocity
    {
        get
        {
            ThrowIfDisposed();
            return Element.TurnVelocity;
        }
        set
        {
            ThrowIfDisposed();
            Element.TurnVelocity = value;
        }
    }

    public bool AreCollisionsEnabled
    {
        get
        {
            ThrowIfDisposed();
            return Element.AreCollisionsEnabled;
        }
        set
        {
            ThrowIfDisposed();
            Element.AreCollisionsEnabled = value;
        }
    }

    public byte Alpha
    {
        get
        {
            ThrowIfDisposed();
            return Element.Alpha;
        }
        set
        {
            ThrowIfDisposed();
            Element.Alpha = value;
        }
    }

    public bool IsFrozen
    {
        get
        {
            ThrowIfDisposed();
            return Element.IsFrozen;
        }
        set
        {
            ThrowIfDisposed();
            Element.IsFrozen = value;
        }
    }

    protected bool IsPerPlayer { get => _isPerPlayer; set => _isPerPlayer = value; }

    protected override void Attach()
    {
        if (Entity.TryGetComponent(out PlayerElementComponent playerElementComponent) && GetType() != typeof(PlayerElementComponent))
        {
            Player = playerElementComponent.Player;
            Element.Id = (ElementId)playerElementComponent.MapIdGenerator.GetId();
            _isPerPlayer = true;
        }
        else
        {
            Bind();
        }
        BaseLoaded = true;
    }


    public void Bind()
    {
        _transform = Entity.GetRequiredComponent<Transform>();
        Element.Position = _transform.Position;
        if (Element.ElementType != ElementType.Pickup)
            Element.Rotation = _transform.Rotation;
        Element.Interior = _transform.Interior;
        Element.Dimension = _transform.Dimension;

        Element.PositionChanged += HandlePositionChanged;
        Element.RotationChanged += HandleRotationChanged;
        Element.InteriorChanged += HandleInteriorChanged;
        Element.DimensionChanged += HandleDimensionChanged;
        _transform.PositionChanged += HandleTransformPositionChanged;
        _transform.RotationChanged += HandleTransformRotationChanged;
        _transform.InteriorChanged += HandleTransformInteriorChanged;
        _transform.DimensionChanged += HandleTransformDimensionChanged;
    }

    private void HandlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        if (_transform == null)
            return;

        if(args.IsSync)
            _transform.SetPosition(args.NewValue, true);
    }

    private void HandleRotationChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        if (_transform == null)
            return;

        if (args.IsSync)
            _transform.SetRotation(args.NewValue, true);
    }

    private void HandleInteriorChanged(Element sender, ElementChangedEventArgs<byte> args)
    {
        if (_transform == null)
            return;

        if (args.IsSync)
            _transform.SetInterior(args.NewValue, true);
    }

    private void HandleDimensionChanged(Element sender, ElementChangedEventArgs<ushort> args)
    {
        if (_transform == null)
            return;

        if (args.IsSync)
            _transform.SetDimension(args.NewValue, true);
    }

    private void HandleTransformPositionChanged(Transform newTransform, Vector3 position, bool sync)
    {
        if (!_isPerPlayer)
            Element.Position = position;
    }

    private void HandleTransformRotationChanged(Transform newTransform, Vector3 rotation, bool sync)
    {
        if (!_isPerPlayer)
            Element.Rotation = rotation;
    }
    
    private void HandleTransformInteriorChanged(Transform newTransform, byte interior, bool sync)
    {
        if (!_isPerPlayer)
            Element.Interior = interior;
    }
    
    private void HandleTransformDimensionChanged(Transform newTransform, ushort dimension, bool sync)
    {
        if (!_isPerPlayer)
            Element.Dimension = dimension;
    }

    protected override void Detach()
    {
        if (!_isPerPlayer)
        {
            if(_transform != null)
            {
                Element.PositionChanged -= HandlePositionChanged;
                Element.RotationChanged -= HandleRotationChanged;
                Element.InteriorChanged -= HandleInteriorChanged;
                Element.DimensionChanged -= HandleDimensionChanged;
                _transform.PositionChanged -= HandleTransformPositionChanged;
                _transform.RotationChanged -= HandleTransformRotationChanged;
                _transform.InteriorChanged -= HandleTransformInteriorChanged;
                _transform.DimensionChanged -= HandleTransformDimensionChanged;
            }
        }
        if (Player != null)
        {
            Element.DestroyFor(Player);
        }
        else
        {
            if (Element is Vehicle vehicle)
            {
                foreach (var occupant in vehicle.Occupants)
                    vehicle.RemovePassenger(occupant.Value);
            }
            var destroyed = Element.Destroy();
        }
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
