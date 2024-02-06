namespace RealmCore.Server.Services;

public interface INewsService
{
    Task<List<NewsDTO>> Get(int limit = 10, CancellationToken cancellationToken = default);
}

internal sealed class NewsService : INewsService
{
    private readonly INewsRepository _newsRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public NewsService(INewsRepository newsRepository, IDateTimeProvider dateTimeProvider)
    {
        _newsRepository = newsRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<List<NewsDTO>> Get(int limit = 10, CancellationToken cancellationToken = default)
    {
        var newsDataList = await _newsRepository.Get(_dateTimeProvider.Now, limit, cancellationToken);
        return newsDataList.Select(NewsDTO.Map).ToList();
    }
}
