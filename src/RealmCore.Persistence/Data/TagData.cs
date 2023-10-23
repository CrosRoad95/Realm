namespace RealmCore.Persistence.Data;

public class TagData
{
    public int Id { get; set; }
    public string Tag { get; set; }

    public ICollection<NewsTagData> NewsTags { get; set; } = new List<NewsTagData>();
}
