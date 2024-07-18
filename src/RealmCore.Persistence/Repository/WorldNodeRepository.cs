namespace RealmCore.Persistence.Repository;

public interface IWorldNodeRepository
{
    Task<WorldNodeScheduledActionData> AddScheduledAction(int worldNodeId, DateTime scheduledTime, object? actionData);
    Task<WorldNodeData> Create(Vector3 position, Vector3 rotation, byte interior, ushort dimension, DateTime createdAt, DateTime lastUpdateAt, string typeName, object? metadata);
    Task<WorldNodeData[]> GetAll(CancellationToken cancellationToken);
    Task<WorldNodeScheduledActionData[]> GetAllScheduledActions(CancellationToken cancellationToken);
    Task Remove(WorldNodeData worldNodeData);
    Task<bool> RemoveScheduledAction(int worldNodeScheduledActionId);
    Task UpdateMetadata(int worldNodeId, object? metadata, DateTime now);
}

internal sealed class WorldNodeRepository : IWorldNodeRepository
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1);
    private readonly IDb _db;

    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.None,
    };

    public WorldNodeRepository(IDb db)
    {
        _db = db;
    }

    private string Serialize(object? data) => JsonConvert.SerializeObject(data, _jsonSerializerSettings);

    public async Task<WorldNodeData[]> GetAll(CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            using var activity = Activity.StartActivity(nameof(GetAll));

            var query = _db.WorldNodes
                //.Include(x => x.ScheduledActionData)
                .AsNoTracking()
                .TagWithSource(nameof(WorldNodeRepository));

            return await query.ToArrayAsync(cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<WorldNodeScheduledActionData[]> GetAllScheduledActions(CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            using var activity = Activity.StartActivity(nameof(GetAll));

            var query = _db.WorldNodeScheduledActionsData
                //.Include(x => x.ScheduledActionData)
                .AsNoTracking()
                .TagWithSource(nameof(WorldNodeRepository));

            return await query.ToArrayAsync(cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<WorldNodeData> Create(Vector3 position, Vector3 rotation, byte interior, ushort dimension, DateTime createdAt, DateTime lastUpdateAt, string typeName, object? metadata)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            using var activity = Activity.StartActivity(nameof(Create));

            var data = new WorldNodeData
            {
                CreatedAt = createdAt,
                LastUpdatedAt = createdAt,
                TypeName = typeName,
                MetaData = Serialize(metadata),
                Transform = new TransformData
                {
                    Position = position,
                    Rotation = rotation,
                    Interior = interior,
                    Dimension = dimension,
                }
            };

            _db.WorldNodes.Add(data);
            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();

            activity?.AddTag("WorldNodeId", data.Id);

            return data;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task UpdateMetadata(int worldNodeId, object? metadata, DateTime now)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            using var activity = Activity.StartActivity(nameof(UpdateMetadata));
            if (activity != null)
            {
                activity.AddTag("WorldNodeId", worldNodeId);
            }

            var serializedMetadata = Serialize(metadata);
            await _db.WorldNodes.Where(x => x.Id == worldNodeId)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.MetaData, serializedMetadata).SetProperty(y => y.LastUpdatedAt, now));
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<bool> RemoveScheduledAction(int worldNodeScheduledActionId)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            using var activity = Activity.StartActivity(nameof(Remove));
            activity?.AddTag("WorldNodeScheduledActionId", worldNodeScheduledActionId);

            return await _db.WorldNodeScheduledActionsData.Where(x => x.Id == worldNodeScheduledActionId).ExecuteDeleteAsync() == 1;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<WorldNodeScheduledActionData> AddScheduledAction(int worldNodeId, DateTime scheduledTime, object? actionData)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            using var activity = Activity.StartActivity(nameof(AddScheduledAction));
            activity?.AddTag("WorldNodeId", worldNodeId);

            var data = new WorldNodeScheduledActionData
            {
                WorldNodeId = worldNodeId,
                ScheduledTime = scheduledTime,
                ActionData = Serialize(actionData),
            };

            _db.WorldNodeScheduledActionsData.Add(data);
            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();

            activity?.AddTag("WorldNodeScheduledActionId", data.Id);

            return data;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task Remove(WorldNodeData worldNodeData)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            using var activity = Activity.StartActivity(nameof(Remove));
            if (activity != null)
            {
                activity.AddTag("WorldNodeId", worldNodeData.Id);
            }

            await _db.WorldNodes.Where(x => x.Id == worldNodeData.Id).ExecuteDeleteAsync();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public static readonly ActivitySource Activity = new("RealmCore.WorldNodeRepository", "1.0.0");
}
