namespace RealmCore.Persistence.Data;

public class UserEventData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventType { get; set; }
    public string? Metadata { get; set; }
    public DateTime DateTime { get; set; }
}
