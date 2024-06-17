namespace RealmCore.Server.Modules.Server;

public interface INewsService
{
    Task<NewsDto[]> Get(int limit = 10, CancellationToken cancellationToken = default);
}

internal sealed class NewsService : INewsService
{
    private readonly INewsRepository _newsRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IServiceScope _serviceScope;

    public NewsService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _newsRepository = _serviceScope.ServiceProvider.GetRequiredService<INewsRepository>();
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<NewsDto[]> Get(int limit = 10, CancellationToken cancellationToken = default)
    {
        var newsDataList = await _newsRepository.Get(_dateTimeProvider.Now, limit, cancellationToken);
        return [.. newsDataList.Select(NewsDto.Map)];
    }
}
