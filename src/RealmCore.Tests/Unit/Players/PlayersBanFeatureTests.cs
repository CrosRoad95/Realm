namespace RealmCore.Tests.Unit.Players;

public class PlayersBanFeatureTests : RealmUnitTestingBase
{
    [Fact]
    public void BansShouldWork()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();

        //await realmTestingServer.GetRequiredService<IDb>().MigrateAsync();
        var bans = player.Bans;

        bans.Add(type: 0, reason: "sample reason");
        var isBanned = bans.IsBanned(0);
        isBanned.Should().BeTrue();
    }
}
