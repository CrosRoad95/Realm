namespace RealmCore.Persistence.Data;

public class NewsTagData
{
    public int NewsId { get; set; }
    public int TagId { get; set; }

    public NewsData News { get; set; }
    public TagData Tag { get; set; }
}
