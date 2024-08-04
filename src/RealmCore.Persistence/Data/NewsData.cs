namespace RealmCore.Persistence.Data;

public class NewsData
{
    public int Id { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime PublishTime { get; set; }
    public string Title { get; set; }
    public string Excerpt { get; set; }
    public string Content { get; set; }
    public ICollection<NewsTagData> NewsTags { get; set; } = new List<NewsTagData>();
}

public class NewsTagData
{
    public int NewsId { get; set; }
    public int TagId { get; set; }

    public NewsData News { get; set; }
    public TagData Tag { get; set; }
}

public class TagData
{
    public int Id { get; set; }
    public string Tag { get; set; }

    public ICollection<NewsTagData> NewsTags { get; set; } = new List<NewsTagData>();
}
