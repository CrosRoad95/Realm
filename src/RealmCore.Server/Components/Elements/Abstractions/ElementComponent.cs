using RealmCore.ECS.Components;
using SlipeServer.Server.Elements;

namespace RealmCore.Server.Components.Elements.Abstractions;

public abstract class ElementComponent : Component
{
    internal Action<Element>? AddFocusableHandler { get; set; }
    internal Action<Element>? RemoveFocusableHandler { get; set; }

    abstract internal Element Element { get; }
    private Player? Player { get; set; }
    private bool _isPerPlayer = false;

    internal bool BaseLoaded { get; set; } = false;

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

    protected ElementComponent()
    {
    }

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
        var transform = Entity.GetRequiredComponent<Transform>();
        Element.Position = transform.Position;
        if (Element.ElementType != ElementType.Pickup)
            Element.Rotation = transform.Rotation;
        Element.Interior = transform.Interior;
        Element.Dimension = transform.Dimension;

        Entity.Transform.PositionChanged += HandleTransformPositionChanged;
        Entity.Transform.RotationChanged += HandleTransformRotationChanged;
    }

    private void HandleTransformRotationChanged(Transform newTransform, Vector3 rotation, bool sync)
    {
        if (!_isPerPlayer)
            Element.Rotation = rotation;
    }

    private void HandleTransformPositionChanged(Transform newTransform, Vector3 position, bool sync)
    {
        if (!_isPerPlayer)
            Element.Position = position;
    }

    public void AddFocusable()
    {
        ThrowIfDisposed();
        AddFocusableHandler?.Invoke(Element);
    }

    public void RemoveFocusable()
    {
        ThrowIfDisposed();
        RemoveFocusableHandler?.Invoke(Element);
    }

    protected override void Detach()
    {
        RemoveFocusableHandler?.Invoke(Element);
        if (!_isPerPlayer)
        {
            Entity.Transform.PositionChanged -= HandleTransformPositionChanged;
            Entity.Transform.RotationChanged -= HandleTransformRotationChanged;
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
