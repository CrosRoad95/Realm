namespace RealmCore.Persistence.Repository;

public enum DataEventType
{
    Player,
    Vehicle,
    Group,
}

public record struct DataEvent(DataEventType dataEventType, int entityId, int eventType, string? metadata = null);

public sealed class DataEventRepository
{
    private readonly IDb _db;

    public DataEventRepository(IDb db)
    {
        _db = db;
    }

    // TODO: Add bulk variant
    public async Task<EventDataBase> Add(DataEvent dataEvent, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Add));

        if (activity != null)
        {
            activity.AddTag("DateEventType", dataEvent.dataEventType);
            activity.AddTag("EntityId", dataEvent.entityId);
            activity.AddTag("EventType", dataEvent.eventType);
        }

        EventDataBase eventDataBase;
        switch (dataEvent.dataEventType)
        {
            case DataEventType.Player:
                var userEventData = new UserEventData
                {
                    UserId = dataEvent.entityId,
                    EventType = dataEvent.eventType,
                    DateTime = dateTime,
                    Metadata = dataEvent.metadata
                };
                _db.UserEvents.Add(userEventData);
                eventDataBase = userEventData;
                break;
            case DataEventType.Vehicle:
                var vehicleEventData = new VehicleEventData
                {
                    VehicleId = dataEvent.entityId,
                    EventType = dataEvent.eventType,
                    DateTime = dateTime,
                    Metadata = dataEvent.metadata
                };
                _db.VehicleEvents.Add(vehicleEventData);
                eventDataBase = vehicleEventData;
                break;
            case DataEventType.Group:
                var groupEventData = new GroupEventData
                {
                    GroupId = dataEvent.entityId,
                    EventType = dataEvent.eventType,
                    DateTime = dateTime,
                    Metadata = dataEvent.metadata
                };
                _db.GroupsEvents.Add(groupEventData);
                eventDataBase = groupEventData;
                break;
            default:
                throw new NotSupportedException();
        }
        await _db.SaveChangesAsync(cancellationToken);
        return eventDataBase;
    }

    public async Task<EventDataBase[]> Get(DataEventType dataEventType, int entityId, IEnumerable<int>? eventsTypes = null, DateRange? dateRange = null, QueryPage? page = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Get));

        if (activity != null)
        {
            activity.AddTag("DateEventType", dataEventType);
            activity.AddTag("EntityId", entityId);
            activity.AddTag("EventsTypes", eventsTypes);
            activity.AddTag("Page", page);
        }

        IQueryable<EventDataBase> query = dataEventType switch
        {
            DataEventType.Player => _db.UserEvents.AsNoTracking().TagWithSource(nameof(DataEventRepository)).Where(x => x.UserId == entityId),
            DataEventType.Vehicle => _db.VehicleEvents.AsNoTracking().TagWithSource(nameof(DataEventRepository)).Where(x => x.VehicleId == entityId),
            DataEventType.Group => _db.GroupsEvents.AsNoTracking().TagWithSource(nameof(DataEventRepository)).Where(x => x.GroupId == entityId),
            _ => throw new NotSupportedException(),
        };

        if(page != null)
            query = query.Paginate(page.Value);

        if (dateRange != null)
            query = query.InDateTimeRange(dateRange.Value);

        if (eventsTypes != null)
            query = query.Where(x => eventsTypes.Contains(x.EventType));
        
        var results = await query.ToArrayAsync(cancellationToken);
        activity?.AddTag("ResultsNumber", results.Length);
        return results;
    }

    public static readonly ActivitySource Activity = new("RealmCore.DataEventRepository", "1.0.0");
}
