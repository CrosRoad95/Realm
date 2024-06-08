using System.Diagnostics.CodeAnalysis;

namespace RealmCore.Persistence.Dto;

public sealed class NewsDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Excerpt { get; init; }
    public required string Content { get; init; }
    public required DateTime PublishTime { get; init; }
    public required string[] Tags { get; init; }

    [return: NotNullIfNotNull(nameof(newsData))]
    public static NewsDto? Map(NewsData? newsData)
    {
        if (newsData == null)
            return null;

        return new()
        {
            Id = newsData.Id,
            Title = newsData.Title,
            Excerpt = newsData.Excerpt,
            Content = newsData.Content,
            PublishTime = newsData.PublishTime,
            Tags = newsData.NewsTags.Select(x => x.Tag.Tag).ToArray(),
        };
    }
}
