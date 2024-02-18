namespace RealmCore.Sample.Server;

public class TestSaver : IUserDataSaver
{
    public Task SaveAsync(RealmPlayer player, bool firstTime = false, CancellationToken cancellationToken = default)
    {
        if (!firstTime)
        {
            player.Settings.Set(69, player.Name);
        }
        return Task.CompletedTask;
    }
}
