namespace RealmCore.Tests.Unit.Players;

public class PlayersBanFeatureTests : RealmUnitTestingBase
{
    [Fact]
    public void BansShouldWork()
    {
        var player = CreateServerWithOnePlayer();

        var bans = player.Bans;

        bans.Add(type: 0, reason: "sample reason");
        var isBanned = bans.IsBanned(0);
        isBanned.Should().BeTrue();
    }
}
