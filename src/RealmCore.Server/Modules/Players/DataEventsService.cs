using RealmCore.Persistence.Common;

namespace RealmCore.Server.Modules.Players;

public sealed class EventDataDto : IEqualityComparer<EventDataDto>
{
    public required int Id { get; init; }
    public required int EventType { get; init; }
    public required string? Metadata { get; init; }
    public required DateTime DateTime { get; init; }

    public bool Equals(EventDataDto? x, EventDataDto? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] EventDataDto obj) => obj.Id;


    [return: NotNullIfNotNull(nameof(banData))]
    public static EventDataDto? Map(EventDataBase? banData)
    {
        if (banData == null)
            return null;

        return new()
        {
            Id = banData.Id,
            EventType = banData.EventType,
            Metadata = banData.Metadata,
            DateTime = banData.DateTime
        };
    }
}

public sealed class DataEventsService
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly DataEventRepository _dataEventRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly UsersInUse _usersInUse;

    public DataEventsService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider, UsersInUse usersInUse)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _dataEventRepository = _serviceProvider.GetRequiredService<DataEventRepository>();
        _dateTimeProvider = dateTimeProvider;
        _usersInUse = usersInUse;
    }

    public async Task<EventDataDto> Add(DataEvent dataEvent, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var eventDataBase = await _dataEventRepository.Add(dataEvent, _dateTimeProvider.Now, cancellationToken);
            if(dataEvent.dataEventType == DataEventType.Player)
            {
                if(_usersInUse.TryGetPlayerByUserId(dataEvent.entityId, out var player))
                {
                    player.Events.Add((UserEventData)eventDataBase);
                }
            }
            return EventDataDto.Map(eventDataBase);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<EventDataDto[]> Get(DataEventType dataEventType, int entityId, IEnumerable<int>? eventsTypes = null, DateRange? dateRange = null, QueryPage? page = null, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var eventDataBase = await _dataEventRepository.Get(dataEventType, entityId, eventsTypes, dateRange, page, cancellationToken);
            return [..eventDataBase.Select(EventDataDto.Map)];
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}
