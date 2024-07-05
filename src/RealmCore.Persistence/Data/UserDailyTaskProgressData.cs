namespace RealmCore.Persistence.Data;

public sealed class UserDailyTaskProgressData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DailyTaskId { get; set; }
    public float Progress { get; set; }

    public UserData? User { get; set; }
}