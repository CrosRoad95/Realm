namespace Realm.Domain.Components.Elements;

public abstract class ElementComponent : Component
{
    [Inject]
    protected IEntityByElement EntityByElement { get; set; } = default!;
    [Inject]
    protected IRPGServer _rpgServer { get; set; } = default!;

    abstract public Element Element { get; }

    protected readonly Entity? _createForEntity;

    protected ElementComponent(Entity? createForEntity = null)
    {
        _createForEntity = createForEntity;
        if (_createForEntity != null)
            _createForEntity.Destroyed += HandleCreateForEntityDestroyed;
    }

    private void HandleCreateForEntityDestroyed(Entity entity)
    {
        Destroy();
    }

    private void HandleDestroyed(Entity entity)
    {
        Element.Destroy();
    }

    public override Task Load()
    {
        base.Load();
        Entity.Transform.Bind(Element);
        if (_createForEntity != null)
        {
            var player = _createForEntity.GetRequiredComponent<PlayerElementComponent>().Player;
            Element.CreateFor(player);
        }
        else
            _rpgServer.AssociateElement(new ElementHandle(Element));
        Entity.Destroyed += HandleDestroyed;
        return Task.CompletedTask;
    }
}
