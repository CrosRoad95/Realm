namespace RealmCore.Tests.Tests.Components;

public class StatisticsCounterComponentTests
{
    [Fact]
    public void IncreaseStatAndGetStatShouldWork()
    {
        Entity entity = new();
        StatisticsCounterComponent statisticsCounterComponent = new();
        entity.AddComponent(statisticsCounterComponent);
        using var statisticsCounterComponentMonitor = statisticsCounterComponent.Monitor();

        statisticsCounterComponent.GetStatsIds.Should().BeEmpty();

        statisticsCounterComponent.IncreaseStat(1, 10);
        statisticsCounterComponent.IncreaseStat(1, 10);
        statisticsCounterComponent.IncreaseStat(2, 10);

        statisticsCounterComponent.GetStat(1).Should().Be(20);
        statisticsCounterComponent.GetStat(2).Should().Be(10);
        statisticsCounterComponent.GetStatsIds.Order().Should().BeEquivalentTo(new[] { 1, 2 });
        statisticsCounterComponentMonitor.GetOccurredEvents().Should().BeEquivalentTo(new List<string> { "StatIncreased", "StatIncreased", "StatIncreased" });
    }

    [Fact]
    public void DecreaseAndSetStatShouldWork()
    {
        Entity entity = new();
        StatisticsCounterComponent statisticsCounterComponent = new(new Dictionary<int, float>
        {
            [1] = 10,
            [2] = 10,
            [3] = 10,
        });
        entity.AddComponent(statisticsCounterComponent);
        using var statisticsCounterComponentMonitor = statisticsCounterComponent.Monitor();

        statisticsCounterComponent.GetStatsIds.Should().BeEquivalentTo(new int[] { 1, 2, 3 });

        statisticsCounterComponent.DecreaseStat(1, 5);
        statisticsCounterComponent.SetStat(2, 5);
        statisticsCounterComponent.SetStat(3, 15);
        statisticsCounterComponent.SetStat(3, 15); // Does nothing

        statisticsCounterComponent.Statistics.Should().BeEquivalentTo(new Dictionary<int, float>
        {
            [1] = 5,
            [2] = 5,
            [3] = 15,
        });
        statisticsCounterComponentMonitor.GetOccurredEvents().Should().BeEquivalentTo(new List<string> { "StatDecreased", "StatDecreased", "StatIncreased" });
    }
}
