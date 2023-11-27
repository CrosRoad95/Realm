using System.Diagnostics.CodeAnalysis;

namespace RealmCore.Persistence.DTOs;

public class NewsDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Excerpt { get; set; }
    public string Content { get; set; }
    public DateTime PublishTime { get; set; }
    public string[] Tags { get; set; }

    [return: NotNullIfNotNull(nameof(newsData))]
    public static NewsDTO? Map(NewsData? newsData)
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
