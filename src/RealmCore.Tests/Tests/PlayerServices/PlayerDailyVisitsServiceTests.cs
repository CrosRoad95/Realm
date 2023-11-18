namespace RealmCore.Tests.Tests.PlayerServices;

public class PlayerDailyVisitsServiceTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task VisitCounterShouldUpdateAppropriately(bool useNowDateTime)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        await realmTestingServer.SignInPlayer(player);

        var testDateTimeProvider = realmTestingServer.TestDateTimeProvider;
        var dailyVisitsCounterComponent = player.DailyVisits;

        dailyVisitsCounterComponent.LastVisit = useNowDateTime ? DateTime.Now : DateTime.MinValue;
        int _day = 0;
        bool _reset = false;
        int? _record = 0;
        dailyVisitsCounterComponent.Visited += (e, day, wasReset) =>
        {
            _day = day;
            _reset = wasReset;
        };

        dailyVisitsCounterComponent.VisitsRecord += (e, record) =>
        {
            _record = record;
        };

        dailyVisitsCounterComponent.VisitsInRow.Should().Be(0);
        dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(0);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        dailyVisitsCounterComponent.Update(testDateTimeProvider.Now);

        dailyVisitsCounterComponent.VisitsInRow.Should().Be(1);
        dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(1);
        _day.Should().Be(1);
        _reset.Should().BeFalse();
        _record.Should().Be(1);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        dailyVisitsCounterComponent.Update(testDateTimeProvider.Now);

        dailyVisitsCounterComponent.VisitsInRow.Should().Be(2);
        dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(2);
        _day.Should().Be(2);
        _reset.Should().BeFalse();
        _record.Should().Be(2);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(2));
        dailyVisitsCounterComponent.Update(testDateTimeProvider.Now);

        dailyVisitsCounterComponent.VisitsInRow.Should().Be(0);
        dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(2);
        _day.Should().Be(0);
        _reset.Should().BeTrue();
        _record.Should().Be(2);
    }
}
