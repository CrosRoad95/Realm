namespace RealmCore.Tests.Integration.Server;

public class NewsServiceTests
{
    [Fact]
    public async Task AddingAndListingNewsShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var now = hosting.DateTimeProvider.Now;
        var newsService = hosting.GetRequiredService<NewsService>();
        var context = hosting.GetRequiredService<IDb>();

        var tag1 = Guid.NewGuid().ToString();
        var tag2 = Guid.NewGuid().ToString();
        var tag3 = Guid.NewGuid().ToString();

        var tags = new List<TagData>
        {
            new TagData{ Tag = tag1 },
            new TagData{ Tag = tag2 },
            new TagData{ Tag = tag3 },
        };
        var title = Guid.NewGuid().ToString();

        context.News.Add(new NewsData
        {
            Title = $"title1{title}",
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
            Title = $"title2{title}",
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
            Title = $"title2 not visible{title}",
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

        var newsList = await newsService.Get(2);
        newsList.Should().BeEquivalentTo(new List<NewsDto>
        {
            new NewsDto
            {
                Id = -1,
                Title = $"title1{title}",
                Excerpt = "excerpt",
                Content = "content",
                PublishTime = now.AddMinutes(-2),
                Tags = [tag1, tag2]
            },
            new NewsDto
            {
                Id = -1,
                Title = $"title2{title}",
                Excerpt = "excerpt",
                Content = "content",
                PublishTime = now.AddMinutes(-1),
                Tags = [tag2, tag3]
            },
        }, options =>
        {
            options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(20))).WhenTypeIs<DateTime>();
            options.Excluding(ctx => ctx.Id);

            return options;
        });
    }
}
