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
}
