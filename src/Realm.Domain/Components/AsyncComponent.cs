namespace Realm.Domain.Components;

public abstract class AsyncComponent : Component, IAsyncDisposable
{
    protected virtual Task LoadAsync() => Task.CompletedTask;

    internal async Task InternalLoadAsync()
    {
        ThrowIfDisposed();
        await LoadAsync();
    }

    public ValueTask DisposeAsync()
    {
        ThrowIfDisposed();

        return ValueTask.CompletedTask;
    }
}
