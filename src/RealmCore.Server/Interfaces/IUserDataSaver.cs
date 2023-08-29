namespace RealmCore.Server.Interfaces;

public interface IUserDataSaver
{
    Task SaveAsync(Entity entity);
}
