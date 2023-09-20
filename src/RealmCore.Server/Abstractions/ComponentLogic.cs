namespace RealmCore.Server.Abstractions;

public class ComponentLogic<T> where T : IComponent
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

public class ComponentLogic<T1, T2>
    where T1 : IComponent
    where T2 : IComponent
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
            if (component is T1 tComponent)
                ComponentAdded(tComponent);
        foreach (var component in entity.Components)
            if (component is T2 tComponent)
                ComponentAdded(tComponent);
    }

    private void HandleDisposed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
        entity.ComponentDetached -= HandleComponentDetached;
        entity.Disposed -= HandleDisposed;
        foreach (var component in entity.Components.OfType<T1>())
            if (component is T1 tComponent)
                ComponentDetached(component);
        foreach (var component in entity.Components.OfType<T2>())
            if (component is T2 tComponent)
                ComponentDetached(component);
    }

    private void HandleComponentDetached(Component component)
    {
        if (component is T1 t1Component)
            ComponentDetached(t1Component);
        if (component is T2 t2Component)
            ComponentDetached(t2Component);
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is T1 t1Component)
            ComponentAdded(t1Component);
        if (component is T2 t2Component)
            ComponentAdded(t2Component);
    }

    protected virtual void ComponentAdded(T1 component) { }
    protected virtual void ComponentDetached(T1 component) { }
    protected virtual void ComponentAdded(T2 component) { }
    protected virtual void ComponentDetached(T2 component) { }
}
