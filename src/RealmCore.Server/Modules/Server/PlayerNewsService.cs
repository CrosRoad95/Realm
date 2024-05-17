namespace RealmCore.Server.Modules.Server;

public interface INewsService
{
    Task<List<NewsDto>> Get(int limit = 10, CancellationToken cancellationToken = default);
}

internal sealed class PlayerNewsService : INewsService
{
    private readonly INewsRepository _newsRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PlayerNewsService(INewsRepository newsRepository, IDateTimeProvider dateTimeProvider)
    {
        _newsRepository = newsRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<List<NewsDto>> Get(int limit = 10, CancellationToken cancellationToken = default)
    {
        var newsDataList = await _newsRepository.Get(_dateTimeProvider.Now, limit, cancellationToken);
        return newsDataList.Select(NewsDto.Map).ToList();
    }
}
