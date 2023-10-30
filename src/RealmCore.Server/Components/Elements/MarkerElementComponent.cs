
namespace RealmCore.Server.Components.Elements;

public class MarkerElementComponent : Marker, IElementComponent
{
    private Action<MarkerElementComponent, Entity, Entity>? _entityEntered;
    private Action<MarkerElementComponent, Entity, Entity>? _entityLeft;
    private Action<MarkerElementComponent, Entity, IEntityRule>? _entityRuleFailed;

    public Action<MarkerElementComponent, Entity, Entity>? EntityEntered
    {
        get
        {
            this.ThrowIfDestroyed();
            return _entityEntered;
        }
        set
        {
            this.ThrowIfDestroyed();
            _entityEntered = value;
        }
    }

    public Action<MarkerElementComponent, Entity, Entity>? EntityLeft
    {
        get
        {
            this.ThrowIfDestroyed();
            return _entityLeft;
        }
        set
        {
            this.ThrowIfDestroyed();
            _entityLeft = value;
        }
    }

    public Action<MarkerElementComponent, Entity, IEntityRule>? EntityRuleFailed
    {
        get
        {
            this.ThrowIfDestroyed();
            return _entityRuleFailed;
        }
        set
        {
            this.ThrowIfDestroyed();
            _entityRuleFailed = value;
        }
    }

    public Entity Entity { get; set; }

    private readonly List<IEntityRule> _entityRules = new();
    private CollisionSphere _collisionSphere;
    public CollisionSphere CollisionShape => _collisionSphere;
    public MarkerElementComponent(Vector3 position, float size, MarkerType markerType) : base(position, markerType)
    {
        Size = size;
        _collisionSphere = new CollisionSphere(position, size);
    }

    public void AddRule(IEntityRule entityRule)
    {
        _entityRules.Add(entityRule);
    }

    public void AddRule<TEntityRole>() where TEntityRole : IEntityRule, new()
    {
        this.ThrowIfDestroyed();
        _entityRules.Add(new TEntityRole());
    }

    private void HandleElementEntered(Element element)
    {
        if (EntityEntered == null)
            return;

        if (element.Interior != Interior || element.Dimension != Dimension)
            return;

        //if (!_entityEngine.TryGetByElement(element, out Entity? entity) || entity == null)
        //    return;

        //var tag = entity.GetRequiredComponent<TagComponent>();
        //if (tag is not PlayerTagComponent && tag is not VehicleTagComponent)
        //    return;

        //foreach (var rule in _entityRules)
        //{
        //    if (!rule.Check(entity))
        //    {
        //        EntityRuleFailed?.Invoke(this, entity, rule);
        //        return;
        //    }
        //}

        //try
        //{
        //    EntityEntered(this, Entity, entity);
        //}
        //catch (Exception ex)
        //{
        //    // TODO: log
        //}
    }

    private void HandleElementLeft(Element element)
    {
        if (EntityLeft == null)
            return;

        if (element.Interior != Interior || element.Dimension != Dimension)
            return;

        //if (!_entityEngine.TryGetByElement(element, out Entity? entity) || entity == null)
        //    return;

        //var tag = entity.GetRequiredComponent<TagComponent>();
        //if (tag is not PlayerTagComponent or VehicleTagComponent)
        //    return;

        //if (_entityRules.All(x => x.Check(entity)))
        //{
        //    try
        //    {
        //        EntityLeft(this, Entity, entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        // TODO: log
        //    }
        //}
    }

    public new bool Destroy()
    {
        if (base.Destroy())
        {
            //_collisionShape.ElementEntered -= HandleElementEntered;
            //_collisionShape.ElementLeft -= HandleElementLeft;
            _entityEntered = null;
            _entityLeft = null;
            _entityRuleFailed = null;
            return true;
        }
        return false;
    }

    public void Attach() { }

    public void Detach() { }

    public void Dispose() { }
}
