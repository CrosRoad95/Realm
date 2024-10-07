namespace RealmCore.Persistence.Data;

public class UserNotificationData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Type { get; set; }
    public DateTime SentTime { get; set; }
    public DateTime? ReadTime { get; set; }
    public string Title { get; set; }
    public string Excerpt { get; set; }
    public string Content { get; set; }
}
