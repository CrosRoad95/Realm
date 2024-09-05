namespace RealmCore.Server.Modules.TimeBaseOperations;

public sealed class TimeBaseOperationGroupDto : IEqualityComparer<TimeBaseOperationGroupDto>
{
    private TimeBaseOperationGroupData Data { get; init; }
    public IEnumerable<TimeBaseOperationDto> Operations => Data.Operations?.Select(TimeBaseOperationDto.Map);
    public IEnumerable<BusinessDto> Businesses => Data.Businesses?.Select(x => BusinessDto.Map(x.Business));

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

    public T? GetMetadata<T>() where T : class
    {
        return Data?.Metadata != null ? JsonConvert.DeserializeObject<T>(Data.Metadata, TimeBaseOperationsService._jsonSerializerSettings) : null;
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

    public async Task<TimeBaseOperationGroupDto> CreateGroupForBusiness(int businessId, int category, int limit, object? groupMetadata = null, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationGroupBusinessData timeBaseOperationGroup;
        var groupMetadataString = Serialize(groupMetadata);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            timeBaseOperationGroup = await _timeBaseOperationRepository.CreateGroupForBusiness(businessId, category, limit, groupMetadataString, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
        return TimeBaseOperationGroupDto.Map(timeBaseOperationGroup.OperationGroup);
    }
    
    public async Task<TimeBaseOperationGroupDto> CreateGroup(int category, int limit, object? groupMetadata = null, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationGroupData timeBaseOperationGroup;
        var groupMetadataString = Serialize(groupMetadata);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            timeBaseOperationGroup = await _timeBaseOperationRepository.CreateGroup(category, limit, groupMetadataString, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
        return TimeBaseOperationGroupDto.Map(timeBaseOperationGroup);
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
    
    public async Task<TimeBaseOperationGroupDto[]> GetGroupsByBusinessId(int businessId, bool withOperations = true, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationGroupData[] groups;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            groups = await _timeBaseOperationRepository.GetGroupsByBusinessId(businessId, withOperations, cancellationToken);
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

    public async Task<TimeBaseOperationDto?> CreateOperation(int groupId, int type, int status, DateTime startDateTime, DateTime endDateTime, object? input = null, object? output = null, object? metadata = null, CancellationToken cancellationToken = default)
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
            operation = await _timeBaseOperationRepository.CreateOperation(groupId, type, status, startDateTime, endDateTime, inputString, outputString, metadataString, cancellationToken);
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

    public async Task<TimeBaseOperationDto[]> GetOperationsByUserIdAndBusinessCategory(int userId, int category, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationGroupBusinessData[] timeBaseOperations;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            timeBaseOperations = await _timeBaseOperationRepository.GetOperationsByUserIdAndBusinessCategory(userId, category, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return timeBaseOperations.SelectMany(x => x.OperationGroup?.Operations?.Select(TimeBaseOperationDto.Map)).ToArray();
    }
    
    public async Task<TimeBaseOperationDto[]> GetOperationsGroupId(int groupId, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationData[] timeBaseOperations;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            timeBaseOperations = await _timeBaseOperationRepository.GetOperationsByGroupId(groupId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return timeBaseOperations.Select(TimeBaseOperationDto.Map).ToArray();
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
        string? metadataString;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            metadataString = await _timeBaseOperationRepository.GetGroupMetadata(groupId, cancellationToken);
            if (metadataString == null)
                return null;
        }
        finally
        {
            _semaphore.Release();
        }

        return JsonConvert.DeserializeObject<T>(metadataString, _jsonSerializerSettings);
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

    public async Task<T?> GetOperationMetadata<T>(int operationId, CancellationToken cancellationToken = default) where T : class
    {
        string? metadataString;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            metadataString = await _timeBaseOperationRepository.GetOperationMetadata(operationId, cancellationToken);
            if (metadataString == null)
                return null;
        }
        finally
        {
            _semaphore.Release();
        }

        return JsonConvert.DeserializeObject<T>(metadataString, _jsonSerializerSettings);
    }

    public async Task<bool> SetOperationMetadata(int operationId, object? metadata, CancellationToken cancellationToken = default)
    {
        var metadataString = Serialize(metadata);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _timeBaseOperationRepository.SetOperationMetadata(operationId, metadataString, cancellationToken);
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
