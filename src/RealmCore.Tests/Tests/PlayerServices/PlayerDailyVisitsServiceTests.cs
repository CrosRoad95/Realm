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
        var dailyVisits = player.DailyVisits;

        dailyVisits.LastVisit = useNowDateTime ? DateTime.Now : DateTime.MinValue;
        int _day = 0;
        bool _reset = false;
        int? _record = 0;
        dailyVisits.Visited += (e, day, wasReset) =>
        {
            _day = day;
            _reset = wasReset;
        };

        dailyVisits.VisitsRecord += (e, record) =>
        {
            _record = record;
        };

        dailyVisits.VisitsInRow.Should().Be(1);
        dailyVisits.VisitsInRowRecord.Should().Be(1);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        dailyVisits.Update(testDateTimeProvider.Now);

        dailyVisits.VisitsInRow.Should().Be(2);
        dailyVisits.VisitsInRowRecord.Should().Be(2);
        _day.Should().Be(2);
        _reset.Should().BeFalse();
        _record.Should().Be(2);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        dailyVisits.Update(testDateTimeProvider.Now);

        dailyVisits.VisitsInRow.Should().Be(3);
        dailyVisits.VisitsInRowRecord.Should().Be(3);
        _day.Should().Be(3);
        _reset.Should().BeFalse();
        _record.Should().Be(3);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(2));
        dailyVisits.Update(testDateTimeProvider.Now);

        dailyVisits.VisitsInRow.Should().Be(0);
        dailyVisits.VisitsInRowRecord.Should().Be(3);
        _day.Should().Be(0);
        _reset.Should().BeTrue();
        _record.Should().Be(3);
    }
}
