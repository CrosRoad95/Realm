namespace RealmCore.Tests.Unit.Players;

public class PlayerStatisticsServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly PlayerStatisticsFeature _statistics;

    public PlayerStatisticsServiceTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _statistics = _player.Statistics;
    }

    [Fact]
    public void IncreaseStatAndGetStatShouldWork()
    {
        using var statisticsCounterMonitor = _statistics.Monitor();

        _statistics.StatsIds.Should().BeEmpty();

        _statistics.Increase(1, 10);
        _statistics.Increase(1, 10);
        _statistics.Increase(2, 10);

        using var _ = new AssertionScope();
        _statistics.Get(1).Should().Be(20);
        _statistics.Get(2).Should().Be(10);
        _statistics.StatsIds.Order().Should().BeEquivalentTo([1, 2]);
        statisticsCounterMonitor.GetOccurredEvents().Should().BeEquivalentTo(["Increased", "VersionIncreased", "Increased", "VersionIncreased", "Increased", "VersionIncreased"]);
    }

    [Fact]
    public void DecreaseAndSetStatShouldWork()
    {
        _statistics.Set(1, 10);
        _statistics.Set(2, 10);
        _statistics.Set(3, 20);

        using var statisticsCounterMonitor = _statistics.Monitor();

        _statistics.StatsIds.Should().BeEquivalentTo([1, 2, 3 ]);

        _statistics.Decrease(1, 5);
        _statistics.Set(2, 5);
        _statistics.Set(2, 6);
        _statistics.Set(3, 15);
        _statistics.Set(3, 15); // Does nothing

        using var _ = new AssertionScope();
        _statistics.ToDictionary(x => x.StatId, x => x.Value).Should().BeEquivalentTo(new Dictionary<int, float>
        {
            [1] = 5,
            [2] = 6,
            [3] = 15,
        });
        statisticsCounterMonitor.GetOccurredEvents().Should().BeEquivalentTo(["Decreased", "VersionIncreased", "Decreased", "VersionIncreased", "Increased", "VersionIncreased", "Decreased", "VersionIncreased"]);
    }

    public void Dispose()
    {
        _player.Statistics.Clear();
    }
}
