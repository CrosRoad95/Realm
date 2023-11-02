namespace RealmCore.Server.Abstractions;

public class ComponentLogic<T> where T : IComponent
{
    public ComponentLogic(IElementFactory elementFactory)
    {
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        if (element is not IComponents hasComponents)
            return;
        var components = hasComponents.Components;
        components.ComponentAdded += HandleComponentAdded;
        components.ComponentDetached += HandleComponentDetached;
        element.Destroyed += HandleDestroyed;
        foreach (var component in components.ComponentsList)
            if (component is T tComponent)
                ComponentAdded(tComponent);
    }

    private void HandleDestroyed(Element element)
    {
        if (element is not IComponents hasComponents)
            return;
        var components = hasComponents.Components;
        components.ComponentAdded -= HandleComponentAdded;
        components.ComponentDetached -= HandleComponentDetached;
        element.Destroyed -= HandleDestroyed;
        foreach (var component in components.ComponentsList)
            if (component is T tComponent)
                ComponentDetached(tComponent);
    }

    private void HandleComponentDetached(IComponent component)
    {
        if (component is T tComponent)
            ComponentDetached(tComponent);
    }

    private void HandleComponentAdded(IComponent component)
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
    public ComponentLogic(IElementFactory elementFactory)
    {
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        if (element is not IComponents hasComponents)
            return;
        var components = hasComponents.Components;

        components.ComponentAdded += HandleComponentAdded;
        components.ComponentDetached += HandleComponentDetached;
        element.Destroyed += HandleDestroyed;
        foreach (var component in components.ComponentsList)
            if (component is T1 tComponent)
                ComponentAdded(tComponent);
        foreach (var component in components.ComponentsList)
            if (component is T2 tComponent)
                ComponentAdded(tComponent);
    }

    private void HandleDestroyed(Element element)
    {
        if (element is not IComponents hasComponents)
            return;
        var components = hasComponents.Components;

        components.ComponentAdded -= HandleComponentAdded;
        components.ComponentDetached -= HandleComponentDetached;
        element.Destroyed -= HandleDestroyed;
        foreach (var component in components.ComponentsList)
            if (component is T1 tComponent)
                ComponentDetached(tComponent);
        foreach (var component in components.ComponentsList)
            if (component is T2 tComponent)
                ComponentDetached(tComponent);
    }

    private void HandleComponentDetached(IComponent component)
    {
        if (component is T1 t1Component)
            ComponentDetached(t1Component);
        if (component is T2 t2Component)
            ComponentDetached(t2Component);
    }

    private void HandleComponentAdded(IComponent component)
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
