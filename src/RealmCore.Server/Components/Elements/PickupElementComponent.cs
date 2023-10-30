namespace RealmCore.Server.Components.Elements;

public class PickupElementComponent : Pickup, IElementComponent
{
    internal Action<PickupElementComponent, Element>? ElementEntered { get; set; }
    internal Action<PickupElementComponent, Element>? ElementLeft { get; set; }

    public Action<Entity, Entity>? EntityEntered;
    public Action<Entity, Entity>? EntityLeft;

    public Action<Entity, IEntityRule>? EntityRuleFailed { get; set; }
    public Entity Entity { get; set; }

    private readonly List<IEntityRule> _entityRules = new();

    public PickupElementComponent(Vector3 position, ushort model) : base(position, model)
    {
    }

    public void AddRule(IEntityRule entityRule)
    {
        this.ThrowIfDestroyed();

        _entityRules.Add(entityRule);
    }

    public void AddRule<TEntityRole>() where TEntityRole : IEntityRule, new()
    {
        this.ThrowIfDestroyed();

        _entityRules.Add(new TEntityRole());
    }

    internal bool CheckRules(Entity entity) => _entityRules.All(x => x.Check(entity));

    private void HandleElementEntered(Element element)
    {
        ElementEntered?.Invoke(this, element);
    }
    
    private void HandleElementLeft(Element element)
    {
        ElementLeft?.Invoke(this, element);
    }

    public void Dispose()
    {
        ElementEntered = null;
        ElementLeft = null;
    }

    public void Attach()
    {
        throw new NotImplementedException();
    }

    public void Detach()
    {
        throw new NotImplementedException();
    }
}
