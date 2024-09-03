namespace RealmCore.Server.Modules.TimeBaseOperations;

public sealed class TimeBaseOperationGroupDto : IEqualityComparer<TimeBaseOperationGroupDto>
{
    private TimeBaseOperationGroupData Data { get; init; }
    public IEnumerable<TimeBaseOperationDto> Operations => Data.Operations?.Select(TimeBaseOperationDto.Map);
    public required int Id { get; init; }
    public required int Limit { get; init; }

    public T? GetMetadata<T>() where T : class
    {
        return Data?.Metadata != null ? JsonConvert.DeserializeObject<T>(Data.Metadata, TimeBaseOperationsService._jsonSerializerSettings) : null;
    }

    public bool Equals(TimeBaseOperationGroupDto? x, TimeBaseOperationGroupDto? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] TimeBaseOperationGroupDto obj) => obj.Id;

    [return: NotNullIfNotNull(nameof(timeBaseOperationGroupData))]
    public static TimeBaseOperationGroupDto? Map(TimeBaseOperationGroupData? timeBaseOperationGroupData)
    {
        if (timeBaseOperationGroupData == null)
            return null;

        return new()
        {
            Data = timeBaseOperationGroupData,
            Id = timeBaseOperationGroupData.Id,
            Limit = timeBaseOperationGroupData.Limit
        };
    }
}

public sealed class TimeBaseOperationGroupUserDto : IEqualityComparer<TimeBaseOperationGroupUserDto>
{
    private TimeBaseOperationGroupUserData? Data { get; init; }
    public TimeBaseOperationGroupDto? Group => TimeBaseOperationGroupDto.Map(Data?.Group);
    public required int UserId { get; init; }
    public required int GroupId { get; init; }

    public T? GetUserMetadata<T>() where T: class
    {
        return Data?.Metadata != null ? JsonConvert.DeserializeObject<T>(Data.Metadata, TimeBaseOperationsService._jsonSerializerSettings) : null;
    }
    
    public T? GetMetadata<T>() where T: class
    {
        return Data?.Group?.Metadata != null ? JsonConvert.DeserializeObject<T>(Data.Group.Metadata, TimeBaseOperationsService._jsonSerializerSettings) : null;
    }

    public bool Equals(TimeBaseOperationGroupUserDto? x, TimeBaseOperationGroupUserDto? y) => x?.UserId == y?.UserId && x?.GroupId == y?.GroupId;

    public int GetHashCode([DisallowNull] TimeBaseOperationGroupUserDto obj) => HashCode.Combine(obj.UserId, obj.GroupId);

    [return: NotNullIfNotNull(nameof(timeBaseOperationGroupUserData))]
    public static TimeBaseOperationGroupUserDto? Map(TimeBaseOperationGroupUserData? timeBaseOperationGroupUserData)
    {
        if (timeBaseOperationGroupUserData == null)
            return null;

        return new()
        {
            Data = timeBaseOperationGroupUserData,
            UserId = timeBaseOperationGroupUserData.UserId,
            GroupId = timeBaseOperationGroupUserData.GroupId
        };
    }
}

public sealed class TimeBaseOperationDto : IEqualityComparer<TimeBaseOperationDto>
{
    public required TimeBaseOperationData? Data { get; init; }
    public required int Id { get; init; }
    public required int Type { get; init; }
    public required int Status { get; init; }
    public required DateTime StartDateTime { get; init; }
    public required DateTime EndDateTime { get; init; }

    public T? GetInput<T>() where T : class
    {
        return Data?.Input != null ? JsonConvert.DeserializeObject<T>(Data.Input, TimeBaseOperationsService._jsonSerializerSettings) : null;
    }

    public T? GetOutput<T>() where T : class
    {
        return Data?.Output != null ? JsonConvert.DeserializeObject<T>(Data.Output, TimeBaseOperationsService._jsonSerializerSettings) : null;
    }

    public bool IsCompleted(IDateTimeProvider dateTimeProvider) => dateTimeProvider.Now >= EndDateTime;

    public bool Equals(TimeBaseOperationDto? x, TimeBaseOperationDto? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] TimeBaseOperationDto obj) => obj.Id;

    [return: NotNullIfNotNull(nameof(timeBaseOperation))]
    public static TimeBaseOperationDto? Map(TimeBaseOperationData? timeBaseOperation)
    {
        if (timeBaseOperation == null)
            return null;

        return new()
        {
            Data = timeBaseOperation,
            Id = timeBaseOperation.Id,
            EndDateTime = timeBaseOperation.EndDateTime,
            StartDateTime = timeBaseOperation.StartDateTime,
            Type = timeBaseOperation.Type,
            Status = timeBaseOperation.Status
        };
    }
}

public sealed class TimeBaseOperationsService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ITransactionContext _transactionContext;
    private readonly TimeBaseOperationRepository _timeBaseOperationRepository;

    public TimeBaseOperationsService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _dateTimeProvider = dateTimeProvider;
        _transactionContext = _serviceProvider.GetRequiredService<ITransactionContext>();
        _timeBaseOperationRepository = _serviceProvider.GetRequiredService<TimeBaseOperationRepository>();
    }

    public async Task<TimeBaseOperationGroupUserDto> CreateGroupForUser(int userId, int category, int limit, object? groupUserMetadata = null, object? groupMetadata = null, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationGroupUserData timeBaseOperationGroup;
        var groupUserMetadataString = Serialize(groupUserMetadata);
        var groupMetadataString = Serialize(groupMetadata);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            timeBaseOperationGroup = await _timeBaseOperationRepository.CreateGroupForUser(userId, category, limit, groupUserMetadataString, groupMetadataString, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
        return TimeBaseOperationGroupUserDto.Map(timeBaseOperationGroup);
    }
    
    public async Task<TimeBaseOperationGroupDto?> GetGroupById(int id, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationGroupData? group;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            group = await _timeBaseOperationRepository.GetGroupById(id, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
        return TimeBaseOperationGroupDto.Map(group);
    }
    
    public async Task<TimeBaseOperationGroupDto[]> GetGroupsByUserId(int userId, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationGroupData[] groups;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            groups = await _timeBaseOperationRepository.GetGroupsByUserId(userId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
        return groups.Select(TimeBaseOperationGroupDto.Map).ToArray();
    }
    
    public async Task<TimeBaseOperationGroupDto[]> GetGroupsByCategoryId(int categoryId, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationGroupData[] groups;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            groups = await _timeBaseOperationRepository.GetGroupsByCategoryId(categoryId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
        return groups.Select(TimeBaseOperationGroupDto.Map).ToArray();
    }
    
    public async Task<TimeBaseOperationGroupDto[]> GetGroupsByUserIdAndCategoryId(int userId, int categoryId, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationGroupData[] groups;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            groups = await _timeBaseOperationRepository.GetGroupsByUserIdAndCategoryId(userId, categoryId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
        return groups.Select(TimeBaseOperationGroupDto.Map).ToArray();
    }
    
    public async Task<int?> GetGroupLimitById(int id, CancellationToken cancellationToken = default)
    {
        int? limit;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            limit = await _timeBaseOperationRepository.GetGroupLimitById(id, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
        return limit;
    }

    public async Task<TimeBaseOperationDto?> CreateForUser(int groupId, int userId, int type, int status, DateTime startDateTime, DateTime endDateTime, object? input = null, object? output = null, object? metadata = null, CancellationToken cancellationToken = default)
    {
        if (startDateTime >= endDateTime)
            throw new ArgumentException(null, nameof(startDateTime));

        TimeBaseOperationData? operation;
        var inputString = Serialize(input);
        var outputString = Serialize(output);
        var metadataString = Serialize(metadata);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            operation = await _timeBaseOperationRepository.CreateOperationForUser(groupId, userId, type, status, startDateTime, endDateTime, inputString, outputString, metadataString, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return TimeBaseOperationDto.Map(operation);
    }
    
    public async Task<bool> DeleteOperation(int id, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _timeBaseOperationRepository.DeleteOperation(id, cancellationToken);

        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<TimeBaseOperationDto[]> GetOperationsByUserIdAndCategory(int userId, int category, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationGroupUserData[] timeBaseOperations;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            timeBaseOperations = await _timeBaseOperationRepository.GetOperationsByUserIdAndCategory(userId, category, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return timeBaseOperations.SelectMany(x => x.Group?.Operations?.Select(TimeBaseOperationDto.Map)).ToArray();
    }
    
    public async Task<int> CountOperationsByGroupId(int id, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _timeBaseOperationRepository.CountOperationsByGroupId(id, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<bool> SetStatus(int id, int status, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _timeBaseOperationRepository.SetStatus(id, status, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<T?> GetGroupMetadata<T>(int groupId, CancellationToken cancellationToken = default) where T: class
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var metadataString = await _timeBaseOperationRepository.GetGroupMetadata(groupId, cancellationToken);
            if (metadataString == null)
                return null;

            return JsonConvert.DeserializeObject<T>(metadataString, _jsonSerializerSettings);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<bool> SetGroupMetadata(int groupId, object? metadata, CancellationToken cancellationToken = default)
    {
        var metadataString = Serialize(metadata);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _timeBaseOperationRepository.SetGroupMetadata(groupId, metadataString, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private string? Serialize(object? obj) => obj != null ? JsonConvert.SerializeObject(obj, _jsonSerializerSettings) : null;

    internal static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
    };
}
