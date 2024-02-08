namespace RealmCore.Tests.Tests.PlayerServices;

public class BanServiceTests
{
    [Fact]
    public void BanShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        //await realmTestingServer.GetRequiredService<IDb>().MigrateAsync();
        var bans = player.Bans;

        bans.Add(type: 0, reason: "sample reason");
        var isBanned = bans.IsBanned(0);
        isBanned.Should().BeTrue();
    }
}
