using Realm.Domain.Components;

namespace Realm.Domain;

public class AsyncEntity : Entity, IAsyncDisposable
{
    public override bool IsAsyncEntity => true;

    public AsyncEntity(IServiceProvider serviceProvider, string name = "", EntityTag tag = EntityTag.Unknown) : base(serviceProvider, name, tag)
    {
    }

    public Task<TComponent> AddComponentAsync<TComponent>() where TComponent : AsyncComponent, new()
    {
        return AddComponentAsync(new TComponent());
    }

    public async Task<TComponent> AddComponentAsync<TComponent>(TComponent component) where TComponent : AsyncComponent
    {
        ThrowIfDisposed();
        if (component.Entity != null)
        {
            throw new Exception("Component already attached to other entity");
        }
        InjectProperties(component);
        InternalAddComponent(component);

        try
        {
            component.InternalLoad();
            await component.InternalLoadAsync();
        }
        catch (Exception)
        {
            DestroyComponent(component);
            throw;
        }
        OnComponentAdded(component);
        return component;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();

        return ValueTask.CompletedTask;
    }
}
