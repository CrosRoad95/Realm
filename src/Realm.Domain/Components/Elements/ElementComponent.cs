using SlipeServer.Packets.Structs;

namespace Realm.Domain.Components.Elements;

public abstract class ElementComponent : Component
{
    internal Action<Element>? AddFocusableHandler { get; set; }
    internal Action<Element>? RemoveFocusableHandler { get; set; }

    abstract internal Element Element { get; }
    private Player? Player { get; set; }
    private bool _isPerPlayer = false;

    public bool AreCollisionsEnabled { get => Element.AreCollisionsEnabled; set => Element.AreCollisionsEnabled = value; }
    public Vector3 Velocity { get => Element.Velocity; set => Element.Velocity = value; }
    public Vector3 TurnVelocity { get => Element.TurnVelocity; set => Element.TurnVelocity = value; }
    public byte Alpha { get => Element.Alpha; set => Element.Alpha = value; }
    public bool IsFrozen { get => Element.IsFrozen; set => Element.IsFrozen = value; }
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
        if(Entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            Player = playerElementComponent.Player;
            Element.Id = (ElementId)playerElementComponent.MapIdGenerator.GetId();
            _isPerPlayer = true;
        }
        else
        {
            Entity.Transform.Bind(Element);
            Entity.Disposed += HandleDestroyed;
            Entity.Transform.PositionChanged += HandleTransformPositionChanged;
            Entity.Transform.RotationChanged += HandleTransformRotationChanged;
        }
    }

    private void HandleTransformRotationChanged(Transform newTransform)
    {
        if (!_isPerPlayer)
            Element.Rotation = newTransform.Rotation;
    }

    private void HandleTransformPositionChanged(Transform newTransform)
    {
        if(!_isPerPlayer)
            Element.Position = newTransform.Position;
    }

    public void AddFocusable()
    {
        AddFocusableHandler?.Invoke(Element);
    }

    public void RemoveFocusable()
    {
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
