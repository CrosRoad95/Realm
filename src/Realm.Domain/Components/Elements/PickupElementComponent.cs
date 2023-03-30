using Realm.Domain.Rules;

namespace Realm.Domain.Components.Elements;

public class PickupElementComponent : ElementComponent
{
    [Inject]
    private IECS ECS { get; set; } = default!;
    [Inject]
    private ILogger<PickupElementComponent> Logger { get; set; } = default!;

    protected readonly Pickup _pickup;
    public Action<Entity>? EntityEntered { get; set; }
    public Action<Entity>? EntityLeft { get; set; }
    public Action<Entity, IEntityRule>? EntityRuleFailed { get; set; }

    internal override Element Element => _pickup;

    private readonly List<IEntityRule> _entityRules = new();

    internal PickupElementComponent(Pickup pickup)
    {
        _pickup = pickup;
        _pickup.RespawnTime = 500;
    }

    public void AddRule(IEntityRule entityRule)
    {
        _entityRules.Add(entityRule);
    }

    public void AddRule<TEntityRole>() where TEntityRole: IEntityRule, new()
    {
        _entityRules.Add(new TEntityRole());
    }

    private void HandleElementEntered(Element element)
    {
        if(EntityEntered != null)
            if(ECS.TryGetByElement(element, out var entity))
                if(entity.Tag == EntityTag.Player || entity.Tag == EntityTag.Vehicle)
                {
                    foreach (var rule in _entityRules)
                    {
                        if(!rule.Check(entity))
                        {
                            try
                            {
                                EntityRuleFailed?.Invoke(entity, rule);
                            }
                            catch(Exception ex)
                            {
                                Logger.LogError(ex, "Failed to invoke entity failed callback");
                            }
                            return;
                        }
                    }
                    try
                    {
                        EntityEntered(entity);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Failed to invoke entity entered callback");
                    }
                }
    }

    private void HandleElementLeft(Element element)
    {
        if(EntityLeft != null)
            if (ECS.TryGetByElement(element, out var entity))
                if (entity.Tag == EntityTag.Player || entity.Tag == EntityTag.Vehicle)
                    if (_entityRules.All(x => x.Check(entity)))
                        try
                        {
                            EntityLeft(entity);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Failed to invoke entity left callback");
                        }
    }

    private void HandleDestroyed(Entity entity)
    {
        _pickup.CollisionShape.ElementEntered -= HandleElementEntered;
        _pickup.CollisionShape.ElementLeft -= HandleElementLeft;
        _pickup.CollisionShape.Destroy();
    }

    protected override void Load()
    {
        base.Load();
        Entity.Disposed += HandleDestroyed;
        _pickup.CollisionShape.ElementEntered += HandleElementEntered;
        _pickup.CollisionShape.ElementLeft += HandleElementLeft;
        _pickup.CollisionShape.Position = _pickup.Position;
        Entity.Transform.PositionChanged += HandlePositionChanged;
    }

    private void HandlePositionChanged(Transform transform)
    {
        _pickup.CollisionShape.Position = transform.Position;
    }

    public override void Dispose()
    {
        Entity.Transform.PositionChanged -= HandlePositionChanged;
    }
}
