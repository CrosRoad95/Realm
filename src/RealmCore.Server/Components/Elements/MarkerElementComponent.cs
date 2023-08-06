namespace RealmCore.Server.Components.Elements;

public class MarkerElementComponent : ElementComponent
{
    [Inject]
    private IECS ECS { get; set; } = default!;
    [Inject]
    private ILogger<MarkerElementComponent> Logger { get; set; } = default!;
    [Inject]
    private IElementCollection ElementCollection { get; set; } = default!;

    protected readonly Marker _marker;
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

    internal MarkerElementComponent(Marker marker)
    {
        _marker = marker;
        _collisionShape = new CollisionSphere(marker.Position, _marker.Size);
    }

    public void RefreshColliders()
    {
        ThrowIfDisposed();

        if (_collisionShape is CollisionSphere collisionSphere)
        {
            var elements = ElementCollection.GetWithinRange(_collisionShape.Position, collisionSphere.Radius);
            foreach (var element in elements)
            {
                if (ECS.TryGetByElement(element, out Entity entity))
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

        if (!ECS.TryGetByElement(element, out Entity entity))
            return;

        if (entity.Tag != EntityTag.Player && entity.Tag != EntityTag.Vehicle)
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
            Logger.LogError(ex, "Failed to invoke entity entered callback");
        }
    }

    private void HandleElementLeft(Element element)
    {
        if (EntityLeft == null)
            return;
        if (!ECS.TryGetByElement(element, out Entity entity))
            return;

        if (entity.Tag != EntityTag.Player && entity.Tag != EntityTag.Vehicle)
            return;

        if (_entityRules.All(x => x.Check(entity)))
        {
            try
            {
                EntityLeft(this, Entity, entity);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to invoke entity left callback");
            }
        }
    }

    private void HandlePreDisposed(Entity entity)
    {
        entity.PreDisposed -= HandlePreDisposed;
        _collisionShape.ElementEntered -= HandleElementEntered;
        _collisionShape.ElementLeft -= HandleElementLeft;
        _collisionShape.Destroy();
    }

    protected override void Load()
    {
        base.Load();
        if (Entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
            _collisionShape.Id = (ElementId)playerElementComponent.MapIdGenerator.GetId();

        Entity.PreDisposed += HandlePreDisposed;
        _collisionShape.ElementEntered += HandleElementEntered;
        _collisionShape.ElementLeft += HandleElementLeft;
        _collisionShape.Position = _marker.Position;
        if (!IsPerPlayer)
            Entity.Transform.PositionChanged += HandlePositionChanged;
    }

    private void HandlePositionChanged(Transform transform, Vector3 position)
    {
        _collisionShape.Position = transform.Position;
    }

    public override void Dispose()
    {
        _entityEntered = null;
        _entityLeft = null;
        _entityRuleFailed = null;
        if (!IsPerPlayer)
            Entity.Transform.PositionChanged -= HandlePositionChanged;
        base.Dispose();
    }
}
