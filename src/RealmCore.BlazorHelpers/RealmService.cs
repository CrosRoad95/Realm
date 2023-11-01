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
