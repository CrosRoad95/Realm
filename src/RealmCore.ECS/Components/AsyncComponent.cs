namespace RealmCore.ECS.Components;

public abstract class AsyncComponent : Component, IAsyncDisposable
{
    public override bool IsAsync() => true;

    protected virtual Task LoadAsync() => Task.CompletedTask;

    internal async Task InternalLoadAsync()
    {
        ThrowIfDisposed();
        await LoadAsync();
    }

    public ValueTask DisposeAsync()
    {
        base.Dispose();
        return ValueTask.CompletedTask;
    }
}
