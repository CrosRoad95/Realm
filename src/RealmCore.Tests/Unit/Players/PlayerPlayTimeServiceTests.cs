using RealmCore.Server.Modules.Players.PlayTime;

namespace RealmCore.Tests.Unit.Players;

public class PlayerPlayTimeServiceTests : RealmUnitTestingBase
{
    [Fact]
    public void TestIfCounterWorksCorrectly()
    {
        var server = CreateServer();
        var player1 = CreatePlayer();
        var player2 = CreatePlayer();
        var dateTimeProvider = server.DateTimeProvider;

        var playTime1 = player1.PlayTime;
        var playTime2 = player2.PlayTime;
        playTime2.InternalSetTotalPlayTime(1000);

        playTime1.PlayTime.Should().Be(TimeSpan.Zero);
        playTime1.TotalPlayTime.Should().Be(TimeSpan.Zero);

        playTime2.PlayTime.Should().Be(TimeSpan.Zero);
        playTime2.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(1000));

        dateTimeProvider.AddOffset(TimeSpan.FromSeconds(50));

        playTime1.PlayTime.Should().Be(TimeSpan.FromSeconds(50));
        playTime1.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(50));

        playTime2.PlayTime.Should().Be(TimeSpan.FromSeconds(50));
        playTime2.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(1050));
    }

    [Fact]
    public void CategoryPlayTimeShouldWork()
    {
        var server = CreateServer();
        var player = CreatePlayer();

        var playTime = player.PlayTime;

        playTime.Category = 1;
        server.DateTimeProvider.AddOffset(TimeSpan.FromSeconds(30));
        playTime.Category = 2;
        server.DateTimeProvider.AddOffset(TimeSpan.FromSeconds(30));
        playTime.UpdateCategoryPlayTime(playTime.Category, server.DateTimeProvider.Now);

        playTime.ToList().Should().BeEquivalentTo([
            new PlayerPlayTimeDto(1, TimeSpan.FromSeconds(30)),
            new PlayerPlayTimeDto(2, TimeSpan.FromSeconds(30))
        ]);
    }
}
