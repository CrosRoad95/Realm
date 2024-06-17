namespace RealmCore.Server.Modules.Domain;

// Based on: https://stackoverflow.com/questions/12451609/how-to-await-raising-an-eventhandler-event
public struct AsyncEvent<TEventArgs> where TEventArgs : EventArgs
{
    private readonly List<Func<object, TEventArgs, Task>> _invocationList;
    private readonly object _lock;

    public AsyncEvent()
    {
        _invocationList = [];
        _lock = new();
    }

    public static AsyncEvent<TEventArgs> operator +(
        AsyncEvent<TEventArgs> e, Func<object, TEventArgs, Task> callback)
    {
        if (callback == null) throw new NullReferenceException("callback is null");

        lock (e._lock)
        {
            e._invocationList.Add(callback);
        }
        return e;
    }

    public static AsyncEvent<TEventArgs> operator -(
        AsyncEvent<TEventArgs> e, Func<object, TEventArgs, Task> callback)
    {
        if (callback == null) throw new NullReferenceException("callback is null");

        lock (e._lock)
        {
            e._invocationList.Remove(callback);
        }
        return e;
    }

    public async Task InvokeAsync(object sender, TEventArgs eventArgs)
    {
        Func<object, TEventArgs, Task>[] view;
        lock (_lock)
        {
            view = [.. _invocationList];
        }

        List<Exception> exceptions = [];
        foreach (var callback in view)
        {
            try
            {
                await callback(sender, eventArgs);
            }
            catch(Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
            throw new AggregateException(exceptions);
    }
}