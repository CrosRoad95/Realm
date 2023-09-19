namespace RealmCore.Server.Components.Elements;

public class MarkerElementComponent : ElementComponent
{
    protected readonly Marker _marker;
    private readonly IElementCollection _elementCollection;
    private readonly IEntityEngine _entityEngine;
    protected readonly CollisionSphere _collisionShape;
    internal override Element Element => _marker;
    internal CollisionSphere CollisionShape => _collisionShape;
    private Action<MarkerElementComponent, Entity, Entity>? _entityEntered;
    private Action<MarkerElementComponent, Entity, Entity>? _entityLeft;
    private Action<MarkerElementComponent, Entity, IEntityRule>? _entityRuleFailed;

    public Action<MarkerElementComponent, Entity, Entity>? EntityEntered
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

    public Action<MarkerElementComponent, Entity, Entity>? EntityLeft
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

    public Action<MarkerElementComponent, Entity, IEntityRule>? EntityRuleFailed
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

    public Color Color
    {
        get
        {
            ThrowIfDisposed();
            return _marker.Color;
        }
        set
        {
            ThrowIfDisposed();
            _marker.Color = value;
        }
    }

    public float Size
    {
        get
        {
            ThrowIfDisposed();
            return _marker.Size;
        }
        set
        {
            ThrowIfDisposed();
            _collisionShape.Radius = value / 2.0f;
            _marker.Size = value;
        }
    }

    public Vector3? TargetPosition
    {
        get
        {
            ThrowIfDisposed();
            return _marker.TargetPosition;
        }
        set
        {
            ThrowIfDisposed();
            _marker.TargetPosition = value;
        }
    }

    public MarkerIcon MarkerIcon
    {
        get
        {
            ThrowIfDisposed();
            return _marker.MarkerIcon;
        }
        set
        {
            ThrowIfDisposed();
            _marker.MarkerIcon = value;
        }
    }

    private readonly List<IEntityRule> _entityRules = new();

    internal MarkerElementComponent(Marker marker, IElementCollection elementCollection, IEntityEngine entityEngine)
    {
        _marker = marker;
        _elementCollection = elementCollection;
        _entityEngine = entityEngine;
        _collisionShape = new CollisionSphere(marker.Position, _marker.Size);
    }

    public void RefreshColliders()
    {
        ThrowIfDisposed();

        if (_collisionShape is CollisionSphere collisionSphere)
        {
            var elements = _elementCollection.GetWithinRange(_collisionShape.Position, collisionSphere.Radius);
            foreach (var element in elements)
            {
                if (_entityEngine.TryGetByElement(element, out Entity? entity) && entity != null)
                    CheckCollisionWith(entity);
            }
        }
    }

    public void CheckCollisionWith(Entity entity)
    {
        ThrowIfDisposed();

        if (entity.TryGetComponent(out ElementComponent elementComponent))
        {
            _collisionShape.CheckElementWithin(elementComponent.Element);
        }
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

        if (element.Interior != _marker.Interior || element.Dimension != _marker.Dimension)
            return;

        if (!_entityEngine.TryGetByElement(element, out Entity? entity) || entity == null)
            return;

        var tag = entity.GetRequiredComponent<TagComponent>();
        if (tag is not PlayerTagComponent or VehicleTagComponent)
            return;

        foreach (var rule in _entityRules)
        {
            if (!rule.Check(entity))
            {
                EntityRuleFailed?.Invoke(this, entity, rule);
                return;
            }
        }

        try
        {
            EntityEntered(this, Entity, entity);
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

        if (element.Interior != _marker.Interior || element.Dimension != _marker.Dimension)
            return;

        if (!_entityEngine.TryGetByElement(element, out Entity? entity) || entity == null)
            return;

        var tag = entity.GetRequiredComponent<TagComponent>();
        if (tag is not PlayerTagComponent or VehicleTagComponent)
            return;

        if (_entityRules.All(x => x.Check(entity)))
        {
            try
            {
                EntityLeft(this, Entity, entity);
            }
            catch (Exception ex)
            {
                // TODO: log
            }
        }
    }

    private void HandlePreDisposed(Entity entity)
    {
        entity.PreDisposed -= HandlePreDisposed;
    }

    protected override void Attach()
    {
        base.Attach();
        if (Entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
            _collisionShape.Id = (ElementId)playerElementComponent.MapIdGenerator.GetId();

        Entity.PreDisposed += HandlePreDisposed;
        _collisionShape.ElementEntered += HandleElementEntered;
        _collisionShape.ElementLeft += HandleElementLeft;
        _collisionShape.Position = _marker.Position;
        if (!IsPerPlayer)
            Entity.Transform.PositionChanged += HandlePositionChanged;
    }

    private void HandlePositionChanged(Transform transform, Vector3 position, bool sync)
    {
        _collisionShape.Position = transform.Position;
    }

    protected override void Detach()
    {
        if (!IsPerPlayer)
            Entity.Transform.PositionChanged -= HandlePositionChanged;

        _collisionShape.ElementEntered -= HandleElementEntered;
        _collisionShape.ElementLeft -= HandleElementLeft;

        base.Detach();
    }

    public override void Dispose()
    {
        _entityEntered = null;
        _entityLeft = null;
        _entityRuleFailed = null;
        base.Dispose();
    }
}
