namespace RealmCore.Persistence.DTOs;

public class NewsDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Excerpt { get; set; }
    public string Content { get; set; }
    public DateTime PublishTime { get; set; }
    public string[] Tags { get; set; }
}
