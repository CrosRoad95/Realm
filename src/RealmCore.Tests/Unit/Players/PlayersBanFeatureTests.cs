namespace RealmCore.Tests.Unit.Players;

public class PlayersBanFeatureTests
{
    [Fact]
    public async Task BansShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var sut = player.Bans;

        sut.Add(type: 0, reason: "sample reason");
        var isBanned = sut.IsBanned(0);
        isBanned.Should().BeTrue();
    }
}
