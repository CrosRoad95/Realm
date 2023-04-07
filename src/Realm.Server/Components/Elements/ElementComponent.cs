using SlipeServer.Packets.Structs;

namespace Realm.Server.Components.Elements;

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

    private void HandleDestroyed(Entity entity)
    {
        if (Player != null)
        {
            Element.DestroyFor(Player);
        }
        else
        {
            Entity.Disposed -= HandleDestroyed;
            Element.Destroy();
        }
    }

    protected override void Load()
    {
        if (Entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            Player = playerElementComponent.Player;
            Element.Id = (ElementId)playerElementComponent.MapIdGenerator.GetId();
            _isPerPlayer = true;
        }
        else
        {
            Entity.Transform.Bind(Element);
            Entity.Transform.PositionChanged += HandleTransformPositionChanged;
            Entity.Transform.RotationChanged += HandleTransformRotationChanged;
        }
        BaseLoaded = true;
    }

    private void HandleTransformRotationChanged(Transform newTransform)
    {
        if (!_isPerPlayer)
            Element.Rotation = newTransform.Rotation;
    }

    private void HandleTransformPositionChanged(Transform newTransform)
    {
        if (!_isPerPlayer)
            Element.Position = newTransform.Position;
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

    public override void Dispose()
    {
        RemoveFocusableHandler?.Invoke(Element);
        if (!_isPerPlayer)
        {
            Entity.Transform.PositionChanged -= HandleTransformPositionChanged;
            Entity.Transform.RotationChanged -= HandleTransformRotationChanged;
        }
        HandleDestroyed(Entity);
        base.Dispose();
    }
}
