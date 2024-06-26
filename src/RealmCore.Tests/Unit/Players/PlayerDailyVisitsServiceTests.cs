﻿namespace RealmCore.Tests.Unit.Players;

public class PlayerDailyVisitsServiceTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task VisitCounterShouldUpdateAppropriately(bool useNowDateTime)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer(name: Guid.NewGuid().ToString(), dontLoadData: false);

        var testDateTimeProvider = hosting.DateTimeProvider;
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

        var dayCounter = 1;
        dailyVisits.VisitsInRow.Should().Be(dayCounter);
        dailyVisits.VisitsInRowRecord.Should().Be(1);

        testDateTimeProvider.Add(TimeSpan.FromDays(1));
        dailyVisits.Update(testDateTimeProvider.Now);
        dayCounter++;

        dailyVisits.VisitsInRow.Should().Be(dayCounter);
        dailyVisits.VisitsInRowRecord.Should().Be(2);
        _day.Should().Be(dayCounter);
        _reset.Should().BeFalse();
        _record.Should().Be(2);

        testDateTimeProvider.Add(TimeSpan.FromDays(1));
        dailyVisits.Update(testDateTimeProvider.Now);
        dayCounter++;

        dailyVisits.VisitsInRow.Should().Be(dayCounter);
        dailyVisits.VisitsInRowRecord.Should().Be(3);
        _day.Should().Be(dayCounter);
        _reset.Should().BeFalse();
        _record.Should().Be(3);

        testDateTimeProvider.Add(TimeSpan.FromDays(2));
        dailyVisits.Update(testDateTimeProvider.Now);
        dayCounter = 0;

        dailyVisits.VisitsInRow.Should().Be(dayCounter);
        dailyVisits.VisitsInRowRecord.Should().Be(3);
        _day.Should().Be(0);
        _reset.Should().BeTrue();
        _record.Should().Be(3);
    }
}
