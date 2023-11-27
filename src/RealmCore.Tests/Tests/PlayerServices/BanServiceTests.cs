namespace RealmCore.Tests.Tests.PlayerServices;

public class BanServiceTests
{
    [Fact]
    public async Task BanShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        await realmTestingServer.GetRequiredService<IDb>().MigrateAsync();
        var ban = player.Bans;

        ban.Add(type: 0, reason: "sample reason");
        var isBanned = ban.IsBanned(0);
        isBanned.Should().BeTrue();
    }
}
