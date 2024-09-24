namespace RealmCore.Server.Modules.Server;

public sealed class NewsService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly NewsRepository _newsRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IServiceScope _serviceScope;

    public NewsService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _newsRepository = _serviceScope.ServiceProvider.GetRequiredService<NewsRepository>();
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<NewsDto[]> Get(int limit = 10, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var newsDataList = await _newsRepository.Get(_dateTimeProvider.Now, limit, cancellationToken);
            return [.. newsDataList.Select(NewsDto.Map)];
        }
        finally
        {
            _semaphore.Release();
        }
    }
    public async Task<string[]> GetRecentNewsTitles(DateTime since, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _newsRepository.GetRecentNewsTitles(since, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
