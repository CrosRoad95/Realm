namespace RealmCore.Tests.Unit.Players;

public class PlayerPlayTimeFeatureTests
{
    [Fact]
    public async Task TestIfCounterWorksCorrectly()
    {
        using var hosting = new RealmTestingServerHosting();
        var player1 = await hosting.CreatePlayer();
        var player2 = await hosting.CreatePlayer();

        var dateTimeProvider = hosting.DateTimeProvider;

        var playTime1 = player1.PlayTime;
        var playTime2 = player2.PlayTime;
        playTime2.InternalSetTotalPlayTime(1000);

        playTime1.PlayTime.Should().Be(TimeSpan.Zero);
        playTime1.TotalPlayTime.Should().Be(TimeSpan.Zero);

        playTime2.PlayTime.Should().Be(TimeSpan.Zero);
        playTime2.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(1000));

        dateTimeProvider.Add(TimeSpan.FromSeconds(50));

        playTime1.PlayTime.Should().Be(TimeSpan.FromSeconds(50));
        playTime1.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(50));

        playTime2.PlayTime.Should().Be(TimeSpan.FromSeconds(50));
        playTime2.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(1050));
    }

    [Fact]
    public async Task CategoryPlayTimeShouldWork1()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var playTime = player.PlayTime;

        playTime.Category = 1;
        hosting.DateTimeProvider.Add(TimeSpan.FromSeconds(30));
        playTime.Category = 2;
        hosting.DateTimeProvider.Add(TimeSpan.FromSeconds(30));

        playTime.ToArray().Should().BeEquivalentTo([
            new PlayerPlayTimeDto(1, TimeSpan.FromSeconds(30)),
            new PlayerPlayTimeDto(2, TimeSpan.FromSeconds(30))
        ]);
    }

    [Fact]
    public async Task CategoryPlayTimeShouldWork2()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var playTime = player.PlayTime;

        playTime.Category = 1;
        hosting.DateTimeProvider.Add(TimeSpan.FromSeconds(30));
        hosting.DateTimeProvider.Add(TimeSpan.FromSeconds(30));

        playTime.GetByCategory(1).Should().Be(TimeSpan.FromMinutes(1));
    }
}
