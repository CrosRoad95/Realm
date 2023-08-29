namespace RealmCore.Server.Components.Common;

public class OutlineComponent : Component
{
    private readonly Color _color;

    public Color Color
    {
        get
        {
            ThrowIfDisposed();
            return _color;
        }
    }

    public OutlineComponent(Color color)
    {
        _color = color;
    }

    protected override void Attach()
    {
        var element = Entity.GetRequiredComponent<ElementComponent>();
        element.Detached += HandleDetached;
    }
    
    protected override void Detach()
    {
        var element = Entity.GetRequiredComponent<ElementComponent>();
        element.Detached -= HandleDetached;
    }

    private void HandleDetached(Component component)
    {
        Entity.DestroyComponent(this);
    }

}
