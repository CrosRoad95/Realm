using System.Formats.Asn1;

namespace RealmCore.Tests.Unit.Players;

public class PlayerStatisticsServiceTests
{
    [Fact]
    public async Task IncreaseStatAndGetStatShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var statistics = player.Statistics;

        using var statisticsCounterMonitor = statistics.Monitor();

        statistics.StatsIds.Should().BeEmpty();

        statistics.Increase(1, 10);
        statistics.Increase(1, 10);
        statistics.Increase(2, 10);

        statistics.Get(1).Should().Be(20);
        statistics.Get(2).Should().Be(10);
        statistics.StatsIds.Order().Should().BeEquivalentTo(new[] { 1, 2 });
        statisticsCounterMonitor.GetOccurredEvents().Should().BeEquivalentTo(new List<string> { "Increased", "Increased", "Increased" });
    }

    [Fact]
    public async Task DecreaseAndSetStatShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var statistics = player.Statistics;
        statistics.Set(1, 10);
        statistics.Set(2, 10);
        statistics.Set(3, 20);

        using var statisticsCounterMonitor = statistics.Monitor();

        statistics.StatsIds.Should().BeEquivalentTo(new int[] { 1, 2, 3 });

        statistics.Decrease(1, 5);
        statistics.Set(2, 5);
        statistics.Set(3, 15);
        statistics.Set(3, 15); // Does nothing

        statistics.ToDictionary(x => x.StatId, x => x.Value).Should().BeEquivalentTo(new Dictionary<int, float>
        {
            [1] = 5,
            [2] = 5,
            [3] = 15,
        });
        statisticsCounterMonitor.GetOccurredEvents().Should().BeEquivalentTo(new List<string> { "Decreased", "Decreased", "Increased" });
    }
}
