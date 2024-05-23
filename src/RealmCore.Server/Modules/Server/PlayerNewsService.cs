namespace RealmCore.Server.Modules.Server;

public interface IPlayerNewsService
{
    Task<NewsDto[]> Get(int limit = 10, CancellationToken cancellationToken = default);
}

internal sealed class PlayerNewsService : IPlayerNewsService
{
    private readonly INewsRepository _newsRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PlayerNewsService(INewsRepository newsRepository, IDateTimeProvider dateTimeProvider)
    {
        _newsRepository = newsRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<NewsDto[]> Get(int limit = 10, CancellationToken cancellationToken = default)
    {
        var newsDataList = await _newsRepository.Get(_dateTimeProvider.Now, limit, cancellationToken);
        return [.. newsDataList.Select(NewsDto.Map)];
    }
}
