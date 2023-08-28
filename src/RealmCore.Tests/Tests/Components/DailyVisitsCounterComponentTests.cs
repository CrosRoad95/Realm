using RealmCore.ECS;

namespace RealmCore.Tests.Tests.Components;

public class DailyVisitsCounterComponentTests
{
    private readonly Entity _entity;
    private readonly DailyVisitsCounterComponent _dailyVisitsCounterComponent;
    private readonly TestDateTimeProvider _testDateTimeProvider;

    public DailyVisitsCounterComponentTests()
    {
        _testDateTimeProvider = new();
        var services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();
        _entity = new("test");
        _dailyVisitsCounterComponent = new();
        _entity.AddComponent(_dailyVisitsCounterComponent);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void VisitCounterShouldUpdateAppropriately(bool useNowDateTime)
    {
        _dailyVisitsCounterComponent.LastVisit = useNowDateTime ? DateTime.Now : DateTime.MinValue;
        int _day = 0;
        bool _reset = false;
        int? _record = 0;
        _dailyVisitsCounterComponent.PlayerVisited += (e, day, wasReset) =>
        {
            _day = day;
            _reset = wasReset;
        };

        _dailyVisitsCounterComponent.PlayerVisitsRecord += (e, record) =>
        {
            _record = record;
        };

        _dailyVisitsCounterComponent.VisitsInRow.Should().Be(0);
        _dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(0);

        _testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        _dailyVisitsCounterComponent.Update(_testDateTimeProvider.Now);

        _dailyVisitsCounterComponent.VisitsInRow.Should().Be(1);
        _dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(1);
        _day.Should().Be(1);
        _reset.Should().BeFalse();
        _record.Should().Be(1);

        _testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        _dailyVisitsCounterComponent.Update(_testDateTimeProvider.Now);

        _dailyVisitsCounterComponent.VisitsInRow.Should().Be(2);
        _dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(2);
        _day.Should().Be(2);
        _reset.Should().BeFalse();
        _record.Should().Be(2);

        _testDateTimeProvider.AddOffset(TimeSpan.FromDays(2));
        _dailyVisitsCounterComponent.Update(_testDateTimeProvider.Now);

        _dailyVisitsCounterComponent.VisitsInRow.Should().Be(0);
        _dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(2);
        _day.Should().Be(0);
        _reset.Should().BeTrue();
        _record.Should().Be(2);
    }
}
