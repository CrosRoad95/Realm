namespace RealmCore.Server.Abstractions;

public class ComponentLogic<T> where T: ECS.Interfaces.IComponent
{
    public ComponentLogic(IEntityEngine entityEngine)
    {
        entityEngine.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        entity.ComponentAdded += HandleComponentAdded;
        entity.ComponentDetached += HandleComponentDetached;
        entity.Disposed += HandleDisposed;
        foreach (var component in entity.Components)
            if (component is T tComponent)
                ComponentAdded(tComponent);
    }

    private void HandleDisposed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
        entity.ComponentDetached -= HandleComponentDetached;
        entity.Disposed -= HandleDisposed;
        foreach (var component in entity.Components.OfType<T>())
            if (component is T tComponent)
                ComponentDetached(component);
    }

    private void HandleComponentDetached(Component component)
    {
        if (component is T tComponent)
            ComponentDetached(tComponent);
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is T tComponent)
            ComponentAdded(tComponent);
    }

    protected virtual void ComponentAdded(T component) { }
    protected virtual void ComponentDetached(T component) { }
}
