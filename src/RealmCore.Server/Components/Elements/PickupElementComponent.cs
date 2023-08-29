using RealmCore.Server.Components.Elements.Abstractions;

namespace RealmCore.Server.Components.Elements;

public class PickupElementComponent : ElementComponent
{
    protected readonly Pickup _pickup;
    private readonly IEntityEngine _entityEngine;

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

    private void HandleElementEntered(Element element)
    {
        if (EntityEntered == null)
            return;

        if (!_entityEngine.TryGetByElement(element, out var entity))
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
                // TODO: log
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
                    // TODO: log
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
            // TODO: log
        }
    }

    private void HandleElementLeft(Element element)
    {
        if (EntityLeft == null)
            return;

        if (!_entityEngine.TryGetByElement(element, out var entity))
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
            // TODO: log
        }

        try
        {
            EntityLeft(Entity, entity);
        }
        catch (Exception ex)
        {
            // TODO: log
        }
    }

    private void HandleDisposed(Entity entity)
    {
        Entity.Disposed -= HandleDisposed;
        _pickup.CollisionShape.ElementEntered -= HandleElementEntered;
        _pickup.CollisionShape.ElementLeft -= HandleElementLeft;
        _pickup.CollisionShape.Destroy();
    }

    protected override void Attach()
    {
        base.Attach();
        Entity.Disposed += HandleDisposed;
        _pickup.CollisionShape.ElementEntered += HandleElementEntered;
        _pickup.CollisionShape.ElementLeft += HandleElementLeft;
        _pickup.CollisionShape.Position = _pickup.Position;
        Entity.Transform.PositionChanged += HandlePositionChanged;
    }

    private void HandlePositionChanged(Transform transform, Vector3 position, bool sync)
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
