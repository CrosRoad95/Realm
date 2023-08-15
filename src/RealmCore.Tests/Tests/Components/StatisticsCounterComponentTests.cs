namespace RealmCore.Tests.Tests.Components;

public class StatisticsCounterComponentTests
{
    private readonly Entity _entity;
    private readonly StatisticsCounterComponent _statisticsCounterComponent;

    public StatisticsCounterComponentTests()
    {
        var services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        _entity = new(serviceProvider, "test");
        _statisticsCounterComponent = new();
        _entity.AddComponent(_statisticsCounterComponent);
    }

    [Fact]
    public void IncreaseStatAndGetStatShouldWork()
    {
        _statisticsCounterComponent.GetStatsIds.Should().BeEmpty();

        _statisticsCounterComponent.IncreaseStat(1, 10);
        _statisticsCounterComponent.IncreaseStat(1, 10);
        _statisticsCounterComponent.IncreaseStat(2, 10);

        _statisticsCounterComponent.GetStat(1).Should().Be(20);
        _statisticsCounterComponent.GetStat(2).Should().Be(10);
        _statisticsCounterComponent.GetStatsIds.Order().Should().BeEquivalentTo(new[] { 1, 2 });
    }
}
