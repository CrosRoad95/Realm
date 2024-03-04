namespace RealmCore.Server.Modules.Persistence;

public interface IUserDataSaver
{
    Task SaveAsync(UserData userData, RealmPlayer player, CancellationToken cancellationToken = default);
}
