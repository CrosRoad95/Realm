using RealmCore.Server;

namespace RealmCore.BlazorHelpers;

public class RealmService<T> : IRealmService<T> where T : notnull
{
    public T Service { get; private set; }

    public RealmService(RealmServer realmServer)
    {
        Service = realmServer.GetRequiredService<T>();
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
