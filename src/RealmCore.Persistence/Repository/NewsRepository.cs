using System.Data;

namespace RealmCore.Persistence.Repository;

internal sealed class NewsRepository : INewsRepository
{
    private readonly IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;
    private readonly IDb _db;

    public NewsRepository(IDb db)
    {
        _db = db;
    }

    public async Task Add(string title, string excerpt, string content, DateTime publishTime, string[] tags, int? createdByUserId = null)
    {
        var executionStrategy = _db.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(_isolationLevel, default);
            try
            {
                var newsTags = new List<NewsTagData>();
                foreach (var tagName in tags)
                {
                    var tag = _db.Tags.Where(x => x.Tag == tagName)
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
                _db.News.Add(news);
                await transaction.CommitAsync();
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
            }
        });

        await _db.SaveChangesAsync();
    }

    public async Task<List<NewsData>> Get(DateTime now, int limit = 10)
    {
        var query = _db.News
            .AsNoTracking()
            .TagWithSource()
            .OrderBy(x => x.PublishTime)
            .Where(x => now >= x.PublishTime)
            .Take(limit)
            .Include(x => x.NewsTags)
            .ThenInclude(x => x.Tag);

        return await query.ToListAsync();
    }
}
