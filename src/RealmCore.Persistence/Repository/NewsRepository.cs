using System.Data;

namespace RealmCore.Persistence.Repository;

internal sealed class NewsRepository : INewsRepository
{
    private readonly IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;
    private readonly IDb _db;
    private readonly ITransactionContext _transactionContext;

    public NewsRepository(IDb db, ITransactionContext transactionContext)
    {
        _db = db;
        _transactionContext = transactionContext;
    }

    public async Task Add(string title, string excerpt, string content, DateTime publishTime, string[] tags, int? createdByUserId = null, CancellationToken cancellationToken = default)
    {
        await _transactionContext.ExecuteAsync(async db =>
        {
            var newsTags = new List<NewsTagData>();
            foreach (var tagName in tags)
            {
                var tag = db.Tags.Where(x => x.Tag == tagName)
                    .FirstOrDefault();
                if (tag == null)
                    tag = new TagData { Tag = tagName };
                newsTags.Add(new NewsTagData
                {
                    Tag = tag
                });
            }

            var news = new NewsData
            {
                Title = title,
                Excerpt = excerpt,
                Content = content,
                PublishTime = publishTime,
                CreatedByUserId = createdByUserId,
                NewsTags = newsTags
            };
            db.News.Add(news);
            await db.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

    }

    public async Task<List<NewsData>> Get(DateTime now, int limit = 10, CancellationToken cancellationToken = default)
    {
        var query = _db.News
            .TagWithSource(nameof(UserNotificationRepository))
            .AsNoTracking()
            .OrderByDescending(x => x.PublishTime)
            .Where(x => now >= x.PublishTime)
            .Take(limit)
            .Include(x => x.NewsTags)
            .ThenInclude(x => x.Tag);

        return await query.ToListAsync(cancellationToken);
    }
}
