﻿namespace RealmCore.Persistence.Repository;

public sealed class NewsRepository
{
    private readonly IDb _db;

    public NewsRepository(IDb db)
    {
        _db = db;
    }

    public async Task Add(string title, string excerpt, string content, DateTime publishTime, string[] tags, int? createdByUserId = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Add));

        if (activity != null)
        {
            activity.AddTag("Title", title);
            activity.AddTag("Excerpt", excerpt);
            activity.AddTag("Content", content);
            activity.AddTag("PublishTime", publishTime);
            activity.AddTag("Tags", tags);
            activity.AddTag("CreatedByUserId", createdByUserId);
        }

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
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<NewsData[]> Get(DateTime now, int limit = 10, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Get));

        if (activity != null)
        {
            activity.AddTag("Limit", limit);
        }

        var query = _db.News
            .TagWithSource(nameof(UserNotificationRepository))
            .AsNoTracking()
            .OrderByDescending(x => x.PublishTime)
            .Where(x => now >= x.PublishTime)
            .Take(limit)
            .Include(x => x.NewsTags)
            .ThenInclude(x => x.Tag);

        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<string[]> GetRecentNewsTitles(DateTime since, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetRecentNewsTitles));

        if (activity != null)
        {
            activity.AddTag("Since", since);
        }

        var query = _db.News
            .TagWithSource(nameof(UserNotificationRepository))
            .AsNoTracking()
            .OrderByDescending(x => x.PublishTime)
            .Where(x => since >= x.PublishTime)
            .Select(x => x.Title);

        return await query.ToArrayAsync(cancellationToken);
    }

    public static readonly ActivitySource Activity = new("RealmCore.NewsRepository", "1.0.0");
}
