namespace RealmCore.Server.Modules.TimeBaseOperations;

public sealed class TimeBaseOperationGroupDto : IEqualityComparer<TimeBaseOperationGroupDto>
{
    private TimeBaseOperationGroupData? Data { get; init; }
    public required int Id { get; init; }
    public required int Category { get; init; }
    public required int Limit { get; init; }
    public required object? Metadata { get; init; }

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
            Category = timeBaseOperationGroupData.Category,
            Limit = timeBaseOperationGroupData.Limit,
            Metadata = timeBaseOperationGroupData.Metadata != null ? JsonConvert.DeserializeObject(timeBaseOperationGroupData.Metadata, TimeBaseOperationsService._jsonSerializerSettings) : null,
        };
    }
}

public sealed class TimeBaseOperationForUserDto : IEqualityComparer<TimeBaseOperationForUserDto>
{
    public required TimeBaseOperationDataGroupUserData? Data { get; init; }
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required int Category { get; init; }
    public required int Type { get; init; }
    public required int Status { get; init; }
    public required DateTime StartDateTime { get; init; }
    public required DateTime EndDateTime { get; init; }
    public required object? Input { get; init; }
    public required object? Output { get; init; }
    public required object? Metadata { get; init; }

    public bool IsCompleted(IDateTimeProvider dateTimeProvider) => dateTimeProvider.Now >= EndDateTime;

    public bool Equals(TimeBaseOperationForUserDto? x, TimeBaseOperationForUserDto? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] TimeBaseOperationForUserDto obj) => obj.Id;

    [return: NotNullIfNotNull(nameof(timeBaseOperationDataGroupUser))]
    public static TimeBaseOperationForUserDto? Map(TimeBaseOperationDataGroupUserData? timeBaseOperationDataGroupUser)
    {
        if (timeBaseOperationDataGroupUser == null || timeBaseOperationDataGroupUser.Operation == null || timeBaseOperationDataGroupUser.Group == null)
            return null;

        return new()
        {
            Data = timeBaseOperationDataGroupUser,
            UserId = timeBaseOperationDataGroupUser.UserId,
            Id = timeBaseOperationDataGroupUser.TimeBasedOperationId,
            Category = timeBaseOperationDataGroupUser.Group!.Category,
            EndDateTime = timeBaseOperationDataGroupUser.Operation.EndDateTime,
            StartDateTime = timeBaseOperationDataGroupUser.Operation.StartDateTime,
            Type = timeBaseOperationDataGroupUser.Operation.Type,
            Status = timeBaseOperationDataGroupUser.Operation.Status,
            Input = timeBaseOperationDataGroupUser.Operation?.Input != null ? JsonConvert.DeserializeObject(timeBaseOperationDataGroupUser.Operation.Input, TimeBaseOperationsService._jsonSerializerSettings) : null,
            Output = timeBaseOperationDataGroupUser.Operation?.Output != null ? JsonConvert.DeserializeObject(timeBaseOperationDataGroupUser.Operation.Output, TimeBaseOperationsService._jsonSerializerSettings) : null,
            Metadata = timeBaseOperationDataGroupUser.Metadata != null ? JsonConvert.DeserializeObject(timeBaseOperationDataGroupUser.Metadata, TimeBaseOperationsService._jsonSerializerSettings) : null,
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

    public async Task<TimeBaseOperationGroupDto> CreateGroup(int category, int limit, object? metadata = null, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationGroupData timeBaseOperationGroup;
        var metadataString = JsonConvert.SerializeObject(metadata, _jsonSerializerSettings);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            timeBaseOperationGroup = await _timeBaseOperationRepository.CreateGroup(category, limit, metadataString, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
        return TimeBaseOperationGroupDto.Map(timeBaseOperationGroup);
    }

    public async Task<TimeBaseOperationForUserDto?> CreateForUser(int groupId, int userId, int type, int status, DateTime startDateTime, DateTime endDateTime, object? input = null, object? output = null, object? metadata = null, CancellationToken cancellationToken = default)
    {
        if (startDateTime >= endDateTime)
            throw new ArgumentException(null, nameof(startDateTime));

        TimeBaseOperationDataGroupUserData? timeBaseOperationDataGroupUser;
        var inputString = JsonConvert.SerializeObject(input, _jsonSerializerSettings);
        var outputString = JsonConvert.SerializeObject(output, _jsonSerializerSettings);
        var metadataString = JsonConvert.SerializeObject(metadata, _jsonSerializerSettings);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            timeBaseOperationDataGroupUser = await _timeBaseOperationRepository.CreateOperationForUser(groupId, userId, type, status, startDateTime, endDateTime, inputString, outputString, metadataString, cancellationToken);

        }
        finally
        {
            _semaphore.Release();
        }

        return TimeBaseOperationForUserDto.Map(timeBaseOperationDataGroupUser);
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

    public async Task<TimeBaseOperationForUserDto[]> GetByUserIdAndCategory(int userId, int category, CancellationToken cancellationToken = default)
    {
        TimeBaseOperationDataGroupUserData[] timeBaseOperations;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            timeBaseOperations = await _timeBaseOperationRepository.GetOperationsByUserIdAndCategory(userId, category, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return timeBaseOperations.Select(TimeBaseOperationForUserDto.Map).ToArray();
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

    internal static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
    };
}
