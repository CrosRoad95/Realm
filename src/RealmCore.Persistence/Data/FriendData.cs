namespace RealmCore.Persistence.Data;

public sealed class FriendData
{
    public int UserId1 { get; set; }
    public int UserId2 { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserData? User1 { get; set; }
    public UserData? User2 { get; set; }
}
