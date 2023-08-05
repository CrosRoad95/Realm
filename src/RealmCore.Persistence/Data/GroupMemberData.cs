namespace RealmCore.Persistence.Data;

public sealed class GroupMemberData
{
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public int Rank { get; set; }
    public string RankName { get; set; }

    public GroupData? Group { get; set; }
    public UserData? User { get; set; }
}
