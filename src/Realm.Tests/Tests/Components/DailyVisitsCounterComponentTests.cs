using Realm.Domain.Components.Players;
using Realm.Domain.Options;
using Realm.Domain;
using Realm.Common.Providers;
using Realm.Tests.Providers;
using FluentAssertions;

namespace Realm.Tests.Tests.Components;

public class DailyVisitsCounterComponentTests
{
    private readonly Entity _entity;
    private readonly DailyVisitsCounterComponent _dailyVisitsCounterComponent;
    private readonly TestDateTimeProvider _testDateTimeProvider;

    public DailyVisitsCounterComponentTests()
    {
        _testDateTimeProvider = new();
        var services = new ServiceCollection();
        services.AddSingleton<IDateTimeProvider>(_testDateTimeProvider);

        var serviceProvider = services.BuildServiceProvider();
        _entity = new(serviceProvider, "test", Entity.EntityTag.Unknown);
        _dailyVisitsCounterComponent = new();
        _entity.AddComponent(_dailyVisitsCounterComponent);
    }

    [Fact]
    public void VisitCounterShouldUpdateAppropriatly()
    {
        int _day = 0;
        bool _reseted = false;
        int? _record = 0;
        _dailyVisitsCounterComponent.PlayerVisited += (e, day, reseted) =>
        {
            _day = day;
            _reseted = reseted;
        };

        _dailyVisitsCounterComponent.PlayerVisitsRecord += (e, record) =>
        {
            _record = record;
        };

        _dailyVisitsCounterComponent.VisitsInRow.Should().Be(0);
        _dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(0);

        _testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        _dailyVisitsCounterComponent.Update();

        _dailyVisitsCounterComponent.VisitsInRow.Should().Be(1);
        _dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(1);
        _day.Should().Be(1);
        _reseted.Should().BeFalse();
        _record.Should().Be(1);

        _testDateTimeProvider.AddOffset(TimeSpan.FromDays(1));
        _dailyVisitsCounterComponent.Update();

        _dailyVisitsCounterComponent.VisitsInRow.Should().Be(2);
        _dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(2);
        _day.Should().Be(2);
        _reseted.Should().BeFalse();
        _record.Should().Be(2);

        _testDateTimeProvider.AddOffset(TimeSpan.FromDays(2));
        _dailyVisitsCounterComponent.Update();

        _dailyVisitsCounterComponent.VisitsInRow.Should().Be(0);
        _dailyVisitsCounterComponent.VisitsInRowRecord.Should().Be(2);
        _day.Should().Be(0);
        _reseted.Should().BeTrue();
        _record.Should().Be(2);
    }
}
