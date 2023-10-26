namespace RealmCore.Server.Services;

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
        return newsDataList.Select(x => new NewsDTO
        {
            Id = x.Id,
            Title = x.Title,
            Excerpt = x.Excerpt,
            Content = x.Content,
            PublishTime = x.PublishTime,
            Tags = x.NewsTags.Select(x => x.Tag.Tag).ToArray(),
        }).ToList();
    }
}
