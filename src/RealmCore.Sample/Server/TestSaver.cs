namespace RealmCore.Sample.Server;

public class TestSaver : IUserDataSaver
{
    public Task SaveAsync(UserData userData, RealmPlayer player, CancellationToken cancellationToken = default)
    {
        player.Settings.Set(69, player.Name);
        return Task.CompletedTask;
    }
}
