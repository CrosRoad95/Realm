namespace RealmCore.Tests.Unit.Players;

public class PlayerDailyVisitsServiceTests : RealmUnitTestingBase
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void VisitCounterShouldUpdateAppropriately(bool useNowDateTime)
    {
        var server = CreateServer();
        var player = CreatePlayer();

        var testDateTimeProvider = server.TestDateTimeProvider;
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

        var dayCounter = 0;
        dailyVisits.VisitsInRow.Should().Be(dayCounter);
        dailyVisits.VisitsInRowRecord.Should().Be(0);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        dailyVisits.Update(testDateTimeProvider.Now);
        dayCounter++;

        dailyVisits.VisitsInRow.Should().Be(dayCounter);
        dailyVisits.VisitsInRowRecord.Should().Be(1);
        _day.Should().Be(dayCounter);
        _reset.Should().BeFalse();
        _record.Should().Be(1);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        dailyVisits.Update(testDateTimeProvider.Now);
        dayCounter++;

        dailyVisits.VisitsInRow.Should().Be(dayCounter);
        dailyVisits.VisitsInRowRecord.Should().Be(2);
        _day.Should().Be(dayCounter);
        _reset.Should().BeFalse();
        _record.Should().Be(2);

        testDateTimeProvider.AddOffset(TimeSpan.FromDays(2));
        dailyVisits.Update(testDateTimeProvider.Now);
        dayCounter = 0;

        dailyVisits.VisitsInRow.Should().Be(dayCounter);
        dailyVisits.VisitsInRowRecord.Should().Be(2);
        _day.Should().Be(0);
        _reset.Should().BeTrue();
        _record.Should().Be(2);
    }
}
