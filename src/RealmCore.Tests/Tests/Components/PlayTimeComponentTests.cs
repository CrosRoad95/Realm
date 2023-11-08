namespace RealmCore.Tests.Tests.Components;

public class PlayTimeComponentTests
{
    [Fact]
    public void TestIfCounterWorksCorrectly()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var dateTimeProvider = realmTestingServer.TestDateTimeProvider;

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
