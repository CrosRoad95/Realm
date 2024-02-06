using SlipeServer.Server;

namespace RealmCore.BlazorHelpers;

public interface IRealmService<T>
{
    T Service { get; }
}

public class RealmService<T> : IRealmService<T> where T : notnull
{
    public T Service { get; private set; }

    public RealmService(MtaServer mtaServer)
    {
        Service = mtaServer.GetRequiredService<T>();
    }
}

public class RealmPlayerService<T> : IRealmPlayerService<T> where T : notnull
{
    public T Service { get; private set; }

    public RealmPlayerService(CurrentPlayerContext currentPlayerContext)
    {
        Service = currentPlayerContext.Player.ServiceProvider.GetRequiredService<T>();
    }
}
