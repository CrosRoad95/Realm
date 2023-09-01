namespace RealmCore.Server.Components.Elements;

public abstract class PlayerPrivateElementComponentBase : ElementComponent { }

public class PlayerPrivateElementComponent<TElementComponent> : PlayerPrivateElementComponentBase where TElementComponent : ElementComponent
{
    internal override Element Element => _elementComponent.Element;

    private readonly TElementComponent _elementComponent;

    public Vector3 Position
    {
        get
        {
            ThrowIfDisposed();
            return Element.Position;
        }
        set
        {
            ThrowIfDisposed();
            Element.Position = value;
        }
    }

    public Vector3 Rotation
    {
        get
        {
            ThrowIfDisposed();
            return Element.Rotation;
        }
        set
        {
            ThrowIfDisposed();
            Element.Rotation = value;
        }
    }

    public TElementComponent ElementComponent
    {
        get
        {
            ThrowIfDisposed();
            return _elementComponent;
        }
    }

    public PlayerPrivateElementComponent(TElementComponent elementComponent)
    {
        _elementComponent = elementComponent;
        _elementComponent.Detached += HandleDetachedFromEntity;
    }

    private void HandleDetachedFromEntity(Component elementComponent)
    {
        _elementComponent.Detached -= HandleDetachedFromEntity;
        Entity.DestroyComponent(this);
    }

    protected override void Attach()
    {
        base.Attach();

        if (!_elementComponent.TrySetEntity(Entity))
            throw new ComponentCanNotBeAddedException<PlayerPrivateElementComponent>();

        _elementComponent.InternalAttach();
    }

    protected override void Detach()
    {
        _elementComponent.Detached -= HandleDetachedFromEntity;
        Entity.TryDestroyComponent(_elementComponent);
    }
}

class PlayerPrivateElementComponent : PlayerPrivateElementComponent<ElementComponent>
{
    public PlayerPrivateElementComponent(ElementComponent elementComponent) : base(elementComponent)
    {
    }

    public static PlayerPrivateElementComponent<T> Create<T>(T component) where T : ElementComponent
    {
        return new PlayerPrivateElementComponent<T>(component);
    }
}