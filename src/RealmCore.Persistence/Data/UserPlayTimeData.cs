namespace RealmCore.Persistence.Data;

public class UserPlayTimeData
{
    public int UserId { get; set; }
    public int Category { get; set; }
    public int PlayTime { get; set; }
    public UserData? User { get; set; }
}
