namespace Realm.Domain.Components.Elements;

public abstract class ElementComponent : Component
{
    [Inject]
    protected IEntityByElement EntityByElement { get; set; } = default!;
    [Inject]
    protected IRPGServer _rpgServer { get; set; } = default!;

    abstract internal Element Element { get; }
    private Player? Player { get; set; }

    protected ElementComponent()
    {
    }

    private Task HandleDestroyed(Entity entity)
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
        return Task.CompletedTask;
    }

    public override void Destroy()
    {
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
}
