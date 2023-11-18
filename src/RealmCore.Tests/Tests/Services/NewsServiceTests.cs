namespace RealmCore.Tests.Tests.Services;

public class NewsServiceTests
{
    private readonly RealmTestingServer _server;
    public NewsServiceTests()
    {
        _server = new();
    }

    [Fact]
    public async Task AddingAndListingNewsShouldWork()
    {
        var dateTimeProvider = _server.GetRequiredService<IDateTimeProvider>();
        var now = dateTimeProvider.Now;
        var newsService = _server.GetRequiredService<INewsService>();
        var context = _server.GetRequiredService<IDb>();

        var tags = new List<TagData>
        {
            new TagData{ Tag = "tag1" },
            new TagData{ Tag = "tag2" },
            new TagData{ Tag = "tag3" },
        };

        context.News.Add(new NewsData
        {
            Title = "title1",
            Excerpt = "excerpt",
            Content = "content",
            PublishTime = now.AddMinutes(-2),
            NewsTags = new List<NewsTagData>
            {
                new NewsTagData
                {
                    Tag = tags[0],
                },
                new NewsTagData
                {
                    Tag = tags[1],
                },
            }
        });
        context.News.Add(new NewsData
        {
            Title = "title2",
            Excerpt = "excerpt",
            Content = "content",
            PublishTime = now.AddMinutes(-1),
            NewsTags = new List<NewsTagData>
            {
                new NewsTagData
                {
                    Tag = tags[1],
                },
                new NewsTagData
                {
                    Tag = tags[2],
                },
            }
        });
        context.News.Add(new NewsData
        {
            Title = "title2 not visible",
            Excerpt = "excerpt",
            Content = "content",
            PublishTime = now.AddMinutes(1),
            NewsTags = new List<NewsTagData>
            {
                new NewsTagData
                {
                    Tag = tags[1],
                },
                new NewsTagData
                {
                    Tag = tags[2],
                },
            }
        });
        await context.SaveChangesAsync();

        var newsList = await newsService.Get();
        newsList.Should().BeEquivalentTo(new List<NewsDTO>
        {
            new NewsDTO
            {
                Id = 1,
                Title = "title1",
                Excerpt = "excerpt",
                Content = "content",
                PublishTime = now.AddMinutes(-2),
                Tags = new string[]{ "tag1", "tag2" }
            },
            new NewsDTO
            {
                Id = 2,
                Title = "title2",
                Excerpt = "excerpt",
                Content = "content",
                PublishTime = now.AddMinutes(-1),
                Tags = new string[]{ "tag2", "tag3" }
            },
        });
    }
}
