namespace Realm.Persistance.Data;

public class GroupMember
{
    public int GroupId { get; set; }
    public Guid UserId { get; set; }
    public int Rank { get; set; }
    public string RankName { get; set; }

    public Group? Group { get; set; }
    public User? User { get; set; }
}
