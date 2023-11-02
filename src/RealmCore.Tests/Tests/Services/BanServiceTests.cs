namespace RealmCore.Tests.Tests.Services;

public class BanServiceTests
{
    //[Fact]
    public async Task BanShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        await realmTestingServer.GetRequiredService<IDb>().MigrateAsync();
        var banService = realmTestingServer.GetRequiredService<IBanService>();

        await banService.Ban(player, reason: "sample reason");
        var isBanned = await banService.IsBanned(player);
        isBanned.Should().BeTrue();
    }
}
