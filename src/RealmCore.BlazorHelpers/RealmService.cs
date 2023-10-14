using RealmCore.Server.Interfaces;

namespace RealmCore.BlazorHelpers;

public class RealmService<T> : IRealmService<T> where T : notnull
{
    public T Service { get; private set; }

    public RealmService(IRealmServer realmServer)
    {
        Service = realmServer.GetRequiredService<T>();
    }
}
