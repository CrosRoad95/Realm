namespace RealmCore.Persistence.Data;

public class UserLoginHistoryData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime DateTime { get; set; }
    public string Ip { get; set; }
    public string Serial { get; set; }
}
