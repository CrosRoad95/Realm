namespace RealmCore.Server.Modules.Persistence;

public interface IUserDataSaver
{
    Task SaveAsync(RealmPlayer player, bool firstTime = false, CancellationToken cancellationToken = default);
}
