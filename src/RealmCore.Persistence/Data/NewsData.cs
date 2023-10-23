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
