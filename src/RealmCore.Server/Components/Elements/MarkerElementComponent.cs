namespace RealmCore.Server.Components.Elements;

public class MarkerElementComponent : ElementComponent
{
    [Inject]
    private IECS ECS { get; set; } = default!;

    protected readonly Marker _marker;
    protected readonly CollisionSphere _collisionShape;
    internal override Element Element => _marker;
    internal CollisionSphere CollisionShape => _collisionShape;
    private Action<Entity>? _entityEntered;
    private Action<Entity>? _entityLeft;
    private Action<Entity, IEntityRule>? _entityRuleFailed;

    public Action<Entity>? EntityEntered
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

    public Action<Entity>? EntityLeft
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
                EntityRuleFailed?.Invoke(entity, rule);
                return;
            }
        }
        EntityEntered(entity);
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
            EntityLeft(entity);
    }

    private void HandleDestroyed(Entity entity)
    {
        _collisionShape.ElementEntered -= HandleElementEntered;
        _collisionShape.ElementLeft -= HandleElementLeft;
        _collisionShape.Destroy();
    }

    protected override void Load()
    {
        base.Load();
        if (Entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
            _collisionShape.Id = (ElementId)playerElementComponent.MapIdGenerator.GetId();

        Entity.Disposed += HandleDestroyed;
        _collisionShape.ElementEntered += HandleElementEntered;
        _collisionShape.ElementLeft += HandleElementLeft;
        _collisionShape.Position = _marker.Position;
        if (!IsPerPlayer)
            Entity.Transform.PositionChanged += HandlePositionChanged;
    }

    private void HandlePositionChanged(Transform transform)
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
