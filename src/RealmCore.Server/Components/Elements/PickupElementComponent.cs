namespace RealmCore.Server.Components.Elements;

public class PickupElementComponent : ElementComponent
{
    [Inject]
    private IECS ECS { get; set; } = default!;
    [Inject]
    private ILogger<PickupElementComponent> Logger { get; set; } = default!;

    protected readonly Pickup _pickup;
    internal override Element Element => _pickup;
    private Action<Entity, Entity>? _entityEntered;
    private Action<Entity, Entity>? _entityLeft;
    private Action<Entity, IEntityRule>? _entityRuleFailed;

    public Action<Entity, Entity>? EntityEntered
    {
        get
        {
            ThrowIfDisposed();
            return _entityEntered;
        }
        set
        {
            ThrowIfDisposed();
            _entityEntered = value;
        }
    }

    public Action<Entity, Entity>? EntityLeft
    {
        get
        {
            ThrowIfDisposed();
            return _entityLeft;
        }
        set
        {
            ThrowIfDisposed();
            _entityLeft = value;
        }
    }

    public Action<Entity, IEntityRule>? EntityRuleFailed
    {
        get
        {
            ThrowIfDisposed();
            return _entityRuleFailed;
        }
        set
        {
            ThrowIfDisposed();
            _entityRuleFailed = value;
        }
    }

    private readonly List<IEntityRule> _entityRules = new();

    internal PickupElementComponent(Pickup pickup)
    {
        _pickup = pickup;
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

    private void HandleElementEntered(Element element)
    {
        if (EntityEntered == null)
            return;

        if (!ECS.TryGetByElement(element, out var entity))
            return;

        var tag = entity.GetRequiredComponent<TagComponent>();
        if (tag is not PlayerTagComponent or VehicleTagComponent)
            return;

        foreach (var rule in _entityRules)
        {
            bool success;
            try
            {
                success = rule.Check(entity);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to invoke check role");
                break;
            }

            if (!rule.Check(entity))
            {
                try
                {
                    EntityRuleFailed?.Invoke(entity, rule);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to invoke entity failed callback");
                }
                return;
            }
        }
        try
        {
            EntityEntered(Entity, entity);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to invoke entity entered callback");
        }
    }

    private void HandleElementLeft(Element element)
    {
        if (EntityLeft == null)
            return;

        if (!ECS.TryGetByElement(element, out var entity))
            return;

        var tag = entity.GetRequiredComponent<TagComponent>();
        if (tag is not PlayerTagComponent or VehicleTagComponent)
            return;

        try
        {
            if (!_entityRules.All(x => x.Check(entity)))
                return;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to invoke check role");
        }

        try
        {
            EntityLeft(Entity, entity);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to invoke entity left callback");
        }
    }

    private void HandleDisposed(Entity entity)
    {
        Entity.Disposed -= HandleDisposed;
        _pickup.CollisionShape.ElementEntered -= HandleElementEntered;
        _pickup.CollisionShape.ElementLeft -= HandleElementLeft;
        _pickup.CollisionShape.Destroy();
    }

    protected override void Load()
    {
        base.Load();
        Entity.Disposed += HandleDisposed;
        _pickup.CollisionShape.ElementEntered += HandleElementEntered;
        _pickup.CollisionShape.ElementLeft += HandleElementLeft;
        _pickup.CollisionShape.Position = _pickup.Position;
        Entity.Transform.PositionChanged += HandlePositionChanged;
    }

    private void HandlePositionChanged(Transform transform, Vector3 position)
    {
        _pickup.CollisionShape.Position = position;
    }

    public override void Dispose()
    {
        _entityEntered = null;
        _entityLeft = null;
        _entityRuleFailed = null;
        Entity.Transform.PositionChanged -= HandlePositionChanged;
    }
}
