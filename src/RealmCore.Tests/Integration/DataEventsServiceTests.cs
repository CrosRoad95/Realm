﻿namespace RealmCore.Tests.Integration;

public class DataEventsServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithUniquePlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithUniquePlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly DataEventsService _dataEventsService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DataEventsServiceTests(RealmTestingServerHostingFixtureWithUniquePlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _dataEventsService = _fixture.Hosting.GetRequiredService<DataEventsService>();
        _dateTimeProvider = _fixture.Hosting.GetRequiredService<IDateTimeProvider>();
    }
    
    [Fact]
    public async Task AddingEventShouldWork()
    {
        var eventData = await _dataEventsService.Add(new DataEvent(DataEventType.Player, _player.UserId, 1));
        var eventsData = await _dataEventsService.Get(DataEventType.Player, _player.UserId);

        var expectedEventData = new EventDataDto
        {
            Id = eventData.Id,
            DateTime = _dateTimeProvider.Now,
            EventType = 1,
            Metadata = null
        };

        using var _ = new AssertionScope();

        eventData.Should().BeEquivalentTo(expectedEventData, options => options
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1.Seconds()))
            .WhenTypeIs<DateTime>());

        eventsData.Should().BeEquivalentTo([expectedEventData], options => options
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1.Seconds()))
            .WhenTypeIs<DateTime>());
    }
    
    [Fact]
    public async Task AddingEventShouldWork2()
    {
        var eventData = await _dataEventsService.Add(new DataEvent(DataEventType.Player, _player.UserId, 1));

        _player.Events.ToArray().Length.Should().Be(1);
    }

    public void Dispose()
    {

    }
}
