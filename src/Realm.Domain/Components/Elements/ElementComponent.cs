namespace Realm.Domain.Components.Elements;

public abstract class ElementComponent : Component
{
    [Inject]
    protected IEntityByElement EntityByElement { get; set; } = default!;
    [Inject]
    protected IRPGServer _rpgServer { get; set; } = default!;

    abstract internal Element Element { get; }
    private Player? Player { get; set; }
    private byte _focusableCounter;

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

    public override void Dispose()
    {
        base.Dispose();
        HandleDestroyed(Entity);
    }

    public override Task Load()
    {
        base.Load();
        if(Entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            Player = playerElementComponent.Player;
            Element.Id = playerElementComponent.MapIdGenerator.GetId();
            Element.CreateFor(Player);
        }
        else
        {
            Entity.Transform.Bind(Element);
            _rpgServer.AssociateElement(new ElementHandle(Element));
            Entity.Destroyed += HandleDestroyed;
        }
        return Task.CompletedTask;
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
}
