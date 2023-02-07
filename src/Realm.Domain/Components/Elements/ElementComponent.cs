namespace Realm.Domain.Components.Elements;

public abstract class ElementComponent : Component
{
    [Inject]
    protected IEntityByElement EntityByElement { get; set; } = default!;

    abstract internal Element Element { get; }
    private Player? Player { get; set; }
    private byte _focusableCounter;
    private bool _isPerPlayer = false;

    public bool AreCollisionsEnabled { get => Element.AreCollisionsEnabled; set => Element.AreCollisionsEnabled = value; }

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
            Entity.Destroyed -= HandleDestroyed;
            Element.Destroy();
        }
    }

    protected override void Load()
    {
        if(Entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            Player = playerElementComponent.Player;
            Element.Id = playerElementComponent.MapIdGenerator.GetId();
            _isPerPlayer = true;
        }
        else
        {
            Entity.Transform.Bind(Element);
            Entity.Destroyed += HandleDestroyed;
        }

        Entity.Transform.PositionChanged += HandleTransformPositionChanged;
        Entity.Transform.RotationChanged += HandleTransformRotationChanged;
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
        _focusableCounter++;
        if (_focusableCounter > 0)
            Element.SetData("_focusable", true, DataSyncType.Broadcast);
    }

    public void RemoveFocusable()
    {
        _focusableCounter--;
        if(_focusableCounter == 0)
            Element.SetData("_focusable", false, DataSyncType.Broadcast);
    }

    public override void Dispose()
    {
        if(_focusableCounter != 0)
            Element.SetData("_focusable", false, DataSyncType.Broadcast);
        Entity.Transform.PositionChanged -= HandleTransformPositionChanged;
        Entity.Transform.RotationChanged -= HandleTransformRotationChanged;
        HandleDestroyed(Entity);
        base.Dispose();
    }
}
