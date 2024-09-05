namespace RealmCore.Server.Modules.TimeBaseOperations;

public sealed class BusinessDto : IEqualityComparer<BusinessDto>
{
    private BusinessData Data { get; init; }
    public required int Id { get; init; }
    public required int Category { get; init; }

    public bool Equals(BusinessDto? x, BusinessDto? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] BusinessDto obj) => obj.Id;

    [return: NotNullIfNotNull(nameof(businessData))]
    public static BusinessDto? Map(BusinessData? businessData)
    {
        if (businessData == null)
            return null;

        return new()
        {
            Data = businessData,
            Id = businessData.Id,
            Category = businessData.Category
        };
    }
}

public sealed class BusinessesService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ITransactionContext _transactionContext;
    private readonly BusinessesRepository _businessesRepository;

    public BusinessesService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _dateTimeProvider = dateTimeProvider;
        _transactionContext = _serviceProvider.GetRequiredService<ITransactionContext>();
        _businessesRepository = _serviceProvider.GetRequiredService<BusinessesRepository>();
    }

    public async Task<BusinessDto> Create(int category, object? metadata = null, CancellationToken cancellationToken = default)
    {
        BusinessData business;
        var metadataString = Serialize(metadata);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            business = await _businessesRepository.Create(category, metadataString, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
        return BusinessDto.Map(business);
    }
    
    public async Task<bool> AddBusinessToTimeBaseGroup(int businessId, int groupId, string? metadata = null, CancellationToken cancellationToken = default)
    {
        var metadataString = Serialize(metadata);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _businessesRepository.AddTimeBaseOperationGroup(businessId, groupId, metadataString, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<bool> AddUser(int businessId, int userId, string? metadata = null, CancellationToken cancellationToken = default)
    {
        var metadataString = Serialize(metadata);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _businessesRepository.AddUser(businessId, userId, metadataString, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<BusinessDto?> GetById(int businessId, CancellationToken cancellationToken = default)
    {
        BusinessData? business;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            business = await _businessesRepository.GetById(businessId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return BusinessDto.Map(business);
    }
    
    public async Task<int[]> GetUsersById(int businessId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _businessesRepository.GetUsersById(businessId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<BusinessDto[]> GetByUserId(int userId, CancellationToken cancellationToken = default)
    {
        BusinessData[] businesses;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            businesses = await _businessesRepository.GetByUserId(userId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return businesses.Select(BusinessDto.Map).ToArray();
    }
    
    public async Task<BusinessDto[]> GetByUserIdAndCategory(int userId, int categoryId, CancellationToken cancellationToken = default)
    {
        BusinessData[] businesses;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            businesses = await _businessesRepository.GetByUserIdAndCategory(userId, categoryId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return businesses.Select(BusinessDto.Map).ToArray();
    }
    
    
    public async Task<bool> SetMetadata(int businessId, object? metadata, CancellationToken cancellationToken = default)
    {
        var metadataString = Serialize(metadata);
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _businessesRepository.SetMetadata(businessId, metadataString, cancellationToken);
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
