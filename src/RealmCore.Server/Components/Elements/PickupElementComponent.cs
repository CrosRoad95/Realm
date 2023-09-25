namespace RealmCore.Server.Components.Elements;

public class PickupElementComponent : ElementComponent
{
    protected readonly Pickup _pickup;
    private readonly IEntityEngine _entityEngine;

    internal override Element Element => _pickup;
    internal Pickup Pickup => _pickup;
    internal Action<PickupElementComponent, Element>? ElementEntered { get; set; }
    internal Action<PickupElementComponent, Element>? ElementLeft { get; set; }
    private Action<Entity, IEntityRule>? _entityRuleFailed;

    public Action<Entity, Entity>? EntityEntered;
    public Action<Entity, Entity>? EntityLeft;

    public Action<Entity, IEntityRule>? EntityRuleFailed { get; set; }
    private readonly List<IEntityRule> _entityRules = new();

    internal PickupElementComponent(Pickup pickup, IEntityEngine entityEngine)
    {
        _pickup = pickup;
        _entityEngine = entityEngine;
        _pickup.RespawnTime = 500;
    }

    public void AddRule(IEntityRule entityRule)
    {
        ThrowIfDisposed();

        _entityRules.Add(entityRule);
    }

    public void AddRule<TEntityRole>() where TEntityRole : IEntityRule, new()
    {
        ThrowIfDisposed();

        _entityRules.Add(new TEntityRole());
    }

    internal bool CheckRules(Entity entity) => _entityRules.All(x => x.Check(entity));

    protected override void Detach()
    {
        _pickup.CollisionShape.ElementEntered -= HandleElementEntered;
        _pickup.CollisionShape.ElementLeft -= HandleElementLeft;
        base.Detach();
    }

    protected override void Attach()
    {
        base.Attach();
        _pickup.CollisionShape.ElementEntered += HandleElementEntered;
        _pickup.CollisionShape.ElementLeft += HandleElementLeft;
        _pickup.CollisionShape.Position = _pickup.Position;
        Entity.Transform.PositionChanged += HandlePositionChanged;
    }

    private void HandleElementEntered(Element element)
    {
        ElementEntered?.Invoke(this, element);
    }
    
    private void HandleElementLeft(Element element)
    {
        ElementLeft?.Invoke(this, element);
    }

    private void HandlePositionChanged(Transform transform, Vector3 position, bool sync)
    {
        _pickup.CollisionShape.Position = position;
    }

    public override void Dispose()
    {
        ElementEntered = null;
        ElementLeft = null;
        _entityRuleFailed = null;
        Entity.Transform.PositionChanged -= HandlePositionChanged;
    }
}
