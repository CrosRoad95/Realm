namespace RealmCore.Server.Components.Elements;

public abstract class PlayerPrivateElementComponentBase : Component, IElementComponent { }

public class PlayerPrivateElementComponent<TElementComponent> : PlayerPrivateElementComponentBase where TElementComponent : Element
{
    private readonly TElementComponent _elementComponent;
    internal Element Element => _elementComponent;

    public TElementComponent ElementComponent
    {
        get
        {
            _elementComponent.ThrowIfDestroyed();
            return _elementComponent;
        }
    }

    public PlayerPrivateElementComponent(TElementComponent elementComponent)
    {
        _elementComponent = elementComponent;
        //_elementComponent.Detached += HandleDetachedFromEntity;
    }

    //private void HandleDetachedFromEntity(Component elementComponent)
    //{
    //    _elementComponent.Detached -= HandleDetachedFromEntity;
    //    Entity.DestroyComponent(this);
    //}

    //protected override void Attach()
    //{
    //    base.Attach();

    //    if (!_elementComponent.TrySetEntity(Entity))
    //        throw new ComponentCanNotBeAddedException<PlayerPrivateElementComponent>();

    //    _elementComponent.InternalAttach();
    //}

    //protected override void Detach()
    //{
    //    _elementComponent.Detached -= HandleDetachedFromEntity;
    //    _elementComponent.InternalDetach();
    //    _elementComponent.Dispose();
    //}
}

public class PlayerPrivateElementComponent : PlayerPrivateElementComponent<Element>
{
    public PlayerPrivateElementComponent(Element elementComponent) : base(elementComponent)
    {
    }

    public static PlayerPrivateElementComponent<T> Create<T>(T component) where T : Element, IComponent
    {
        return new PlayerPrivateElementComponent<T>(component);
    }
}