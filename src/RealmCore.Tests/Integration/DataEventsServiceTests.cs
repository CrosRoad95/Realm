namespace RealmCore.Tests.Integration;

public class DataEventsServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithUniquePlayer>, IAsyncDisposable
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
        int eventType = 1;
        var eventData = await _dataEventsService.Add(new DataEvent(DataEventType.Player, _player.UserId, eventType));
        var eventsData = await _dataEventsService.Get(DataEventType.Player, _player.UserId);

        var expectedEventData = new EventDataDto
        {
            Id = eventData.Id,
            DateTime = _dateTimeProvider.Now,
            EventType = eventType,
            Metadata = null
        };

        using var _ = new AssertionScope();

        eventData.Should().BeEquivalentTo(expectedEventData, RealmTestsHelpers.DateTimeCloseTo);
        eventsData.Where(x => x.EventType == eventType).Should().BeEquivalentTo([expectedEventData], RealmTestsHelpers.DateTimeCloseTo);
    }
    
    [Fact]
    public async Task AddingEventShouldWork2()
    {
        int eventType = 2;
        var eventData = await _dataEventsService.Add(new DataEvent(DataEventType.Player, _player.UserId, eventType));

        _player.Events.ToArray().Length.Should().Be(1);
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.Hosting.GetRequiredService<IDb>().UserEvents.Where(x => x.UserId == _player.UserId).ExecuteDeleteAsync();
    }
}
