namespace Realm.Scripting.Extensions;

internal static class ObjectExtensions
{
    public static Task<object> ToTask(this object promise)
    {
        var source = new TaskCompletionSource<object>();
        Action<object> onResolved = result => source.SetResult(result);
        Action<dynamic> onRejected = error => source.SetException(new Exception(error.toString()));
        ((dynamic)promise).then(onResolved, onRejected);
        return source.Task;
    }
}