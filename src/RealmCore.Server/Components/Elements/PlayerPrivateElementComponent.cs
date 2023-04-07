namespace RealmCore.Server.Components.Elements;

public class PlayerPrivateElementComponent<TElementComponent> : ElementComponent where TElementComponent : ElementComponent
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
    }

    protected override void Load()
    {
        base.Load();
        Entity.InjectProperties(_elementComponent);
    }

    public override void Dispose()
    {
        Entity.TryDestroyComponent(ElementComponent);
        base.Dispose();
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