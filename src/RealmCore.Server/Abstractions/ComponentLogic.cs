﻿namespace RealmCore.Server.Abstractions;

public class ComponentLogic<T> where T: Component
{
    public ComponentLogic(IECS ecs)
    {
        ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        entity.ComponentAdded += HandleComponentAdded;
        entity.ComponentDetached += HandleComponentDetached;
        entity.Disposed += HandleDisposed;
    }

    private void HandleDisposed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
        entity.ComponentDetached -= HandleComponentDetached;
        entity.Disposed -= HandleDisposed;
    }

    private void HandleComponentDetached(Component component)
    {
        if (component is T tComponent)
            ComponentRemoved(tComponent);
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is T tComponent)
            ComponentAdded(tComponent);
    }

    protected virtual void ComponentAdded(T component) { }
    protected virtual void ComponentRemoved(T component) { }
}