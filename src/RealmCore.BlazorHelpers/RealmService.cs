namespace RealmCore.BlazorHelpers;

public interface IRealmService<T>
{
    T Service { get; }
}

internal sealed class RealmService<T> : IRealmService<T> where T : notnull
{
    public T Service { get; private set; }

    public RealmService(MtaServer server)
    {
        Service = server.GetRequiredService<T>();
    }
}
