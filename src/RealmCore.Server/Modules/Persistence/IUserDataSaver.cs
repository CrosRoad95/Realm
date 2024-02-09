namespace RealmCore.Server.Modules.Persistence;

public interface IUserDataSaver
{
    Task SaveAsync(RealmPlayer player);
}
