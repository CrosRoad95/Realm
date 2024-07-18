namespace RealmCore.Server.Modules.World.WorldNodes;

internal delegate Task UpdateMetaDataDelegate(int id, object? newMetaData);
internal delegate Task ScheduleActionDelegate(int worldNodeId, DateTime at, object? metadata, int? existingActionId = null);

public abstract class WorldNode
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    internal WorldNodeData WorldNodeData { get; private set; } = default!;
    internal UpdateMetaDataDelegate UpdateMetaDataDelegate { get; private set; } = default!;
    internal ScheduleActionDelegate ScheduleActionDelegate { get; private set; } = default!;
    internal virtual object? Metadata { get; private set; }
    public int Id => WorldNodeData.Id;
    public DateTime CreatedAt => WorldNodeData.CreatedAt;
    public Vector3 Position => WorldNodeData.Transform.Position;
    public Vector3 Rotation => WorldNodeData.Transform.Rotation;

    public CancellationToken Destroyed => _cancellationTokenSource.Token;

    protected virtual Task Initialized() { return Task.CompletedTask; }
    protected void SetLocation(Location location)
    {
        Destroyed.ThrowIfCancellationRequested();

        WorldNodeData.Transform = new TransformData
        {
            Position = location.Position,
            Rotation = location.Rotation,
            Interior = location.GetInteriorOrDefault(),
            Dimension = location.GetDimensionOrDefault()
        };
    }

    protected virtual Task ProcessAction(object? data) => Task.CompletedTask;

    protected virtual void Dispose() { }

    internal async Task InitializedInternal(WorldNodeData worldNodeData, UpdateMetaDataDelegate updateMetaDataDelegate, ScheduleActionDelegate scheduleActionDelegate)
    {
        WorldNodeData = worldNodeData;
        UpdateMetaDataDelegate = updateMetaDataDelegate;
        ScheduleActionDelegate = scheduleActionDelegate;
        Metadata = WorldNodeData.MetaData != null ? JsonConvert.DeserializeObject(WorldNodeData.MetaData, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        }) : null;
        await Initialized();
    }

    protected T GetMetadata<T>() => (T?)Metadata;

    protected async Task UpdateMetadata<T>(Func<T?, T> update)
    {
        Destroyed.ThrowIfCancellationRequested();
        var newMetadata = update((T?)Metadata);
        await UpdateMetaDataDelegate(Id, newMetadata);
        Metadata = newMetadata;
    }
    
    protected async Task ScheduleAction(DateTime scheduleAt, object? actionData)
    {
        Destroyed.ThrowIfCancellationRequested();
        await ScheduleActionDelegate(Id, scheduleAt, actionData);
    }

    internal async Task ProcessActionInternal(object? data) => await ProcessAction(data);
    internal void DisposeInternal()
    {
        _cancellationTokenSource.Cancel();
        Dispose();
    }
}

public sealed class WorldNodesService
{
    private readonly object _lock = new();
    private readonly Dictionary<int, WorldNode> _worldNodesById = [];
    private readonly List<WorldNode> _worldNodes = [];
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISchedulerService _schedulerService;
    private readonly IWorldNodeRepository _worldNodeRepository;
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;

    public WorldNodesService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider, ISchedulerService schedulerService)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _dateTimeProvider = dateTimeProvider;
        _schedulerService = schedulerService;
        _worldNodeRepository = _serviceProvider.GetRequiredService<IWorldNodeRepository>();
    }

    public bool TryGetById<T>(int id, out T node) where T : WorldNode
    {
        lock (_lock)
        {
            if(_worldNodesById.TryGetValue(id, out var value))
            {
                node = (T)value;
                return true;
            }
        }
        node = default!;
        return false;
    }
    
    public WorldNode[] GetAll()
    {
        WorldNode[] view;
        lock (_lock)
        {
            view = [.. _worldNodes];
        }
        return view;
    }
    
    public T[] GetAllOfType<T>() where T: WorldNode
    {
        T[] view;
        lock (_lock)
        {
            view = [.. _worldNodes.OfType<T>()];
        }
        return view;
    }

    internal async Task<WorldNode> CreateFromData(Type nodeType, WorldNodeData worldNodeData)
    {
        WorldNode worldNode = (WorldNode)ActivatorUtilities.CreateInstance(_serviceProvider, nodeType);

        try
        {
            await worldNode.InitializedInternal(worldNodeData, UpdateMetadata, ScheduleAction);
        }
        catch(Exception)
        {
            await _worldNodeRepository.Remove(worldNodeData);
            worldNode.DisposeInternal();
            throw;
        }

        lock (_lock)
        {
            _worldNodes.Add(worldNode);
            _worldNodesById.Add(worldNode.Id, worldNode);
        }

        return worldNode;
    }

    private async Task UpdateMetadata(int worldNodeId, object? metadata)
    {
        await _worldNodeRepository.UpdateMetadata(worldNodeId, metadata, _dateTimeProvider.Now);
    }

    internal async Task ScheduleAction(int worldNodeId, DateTime at, object? actionData, int? existingActionId = null)
    {
        if(at < _dateTimeProvider.Now)
        {
            if(TryGetById(worldNodeId, out WorldNode worldNode))
            {
                try
                {
                    await worldNode.ProcessActionInternal(actionData);
                }
                finally
                {
                    if(existingActionId != null)
                    await _worldNodeRepository.RemoveScheduledAction(existingActionId.Value);
                }
            }
        }
        else
        {
            if(existingActionId == null)
            {
                var scheduledAction = await _worldNodeRepository.AddScheduledAction(worldNodeId, at, actionData);
                existingActionId = scheduledAction.Id;
            }
            _schedulerService.ScheduleJobAt(async () =>
            {
                if (TryGetById(worldNodeId, out WorldNode worldNode))
                {
                    try
                    {
                        await worldNode.ProcessActionInternal(actionData);
                    }
                    finally
                    {
                        await _worldNodeRepository.RemoveScheduledAction(existingActionId.Value);
                    }
                }
            }, at);
        }
    }
    
    private async Task<WorldNode> Create(Type nodeType, Location location, object? metadata)
    {
        var nodeTypeName = $"{nodeType.FullName}, {nodeType.Assembly.GetName().Name}";
        var worldNodeData = await _worldNodeRepository.Create(location.Position, location.Rotation, location.GetInteriorOrDefault(), location.GetDimensionOrDefault(), _dateTimeProvider.Now, _dateTimeProvider.Now, nodeTypeName, metadata);

        return await CreateFromData(nodeType, worldNodeData);
    }

    public async Task<T> Create<T>(Location location, object? metadata) where T: WorldNode
    {
        return (T)await Create(typeof(T), location, metadata);
    }

    public async Task<bool> Destroy(WorldNode node)
    {
        WorldNode? worldNode = null;
        var id = node.Id;
        lock (_lock)
        {
            worldNode = _worldNodes.Where(x => x.Id == id).FirstOrDefault();
            if(worldNode != null)
            {
                _worldNodes.Remove(worldNode);
                _worldNodesById.Remove(id);
            }
        }

        if(worldNode != null)
        {
            await _worldNodeRepository.Remove(worldNode.WorldNodeData);
            worldNode.DisposeInternal();
            return true;
        }

        return false;
    }
}