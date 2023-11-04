namespace RealmCore.Tests.Tests.Components;

public class DailyVisitsCounterComponentTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void VisitCounterShouldUpdateAppropriately(bool useNowDateTime)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var testDateTimeProvider = realmTestingServer.TestDateTimeProvider;
        var dailyVisitsCounterComponent = player.AddComponent<DailyVisitsCounterComponent>();

        dailyVisitsCounterComponent.LastVisit = useNowDateTime ? DateTime.Now : DateTime.MinValue;
        int _day = 0;
        bool _reset = false;
        int? _record = 0;
        dailyVisitsCounterComponent.PlayerVisited += (e, day, wasReset) =>
        {
            _day = day;
            _reset = wasReset;
        };

        dailyVisitsCounterComponent.PlayerVisitsRecord += (e, record) =>
        {
            _record = record;
        };

        dailyVisitsCounterComponent.VisitsInRow.Should().Be(1);
        dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(1);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        dailyVisitsCounterComponent.Update(testDateTimeProvider.Now);

        dailyVisitsCounterComponent.VisitsInRow.Should().Be(2);
        dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(2);
        _day.Should().Be(2);
        _reset.Should().BeFalse();
        _record.Should().Be(2);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        dailyVisitsCounterComponent.Update(testDateTimeProvider.Now);

        dailyVisitsCounterComponent.VisitsInRow.Should().Be(3);
        dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(3);
        _day.Should().Be(3);
        _reset.Should().BeFalse();
        _record.Should().Be(3);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(2));
        dailyVisitsCounterComponent.Update(testDateTimeProvider.Now);

        dailyVisitsCounterComponent.VisitsInRow.Should().Be(0);
        dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(3);
        _day.Should().Be(0);
        _reset.Should().BeTrue();
        _record.Should().Be(3);
    }
}
