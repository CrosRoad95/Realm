namespace RealmCore.Server.Components.Players;

public class GroupMemberComponent : Component
{
    public int GroupId { get; }
    public int Rank { get; }
    public string RankName { get; }

    internal GroupMemberComponent(GroupMemberData groupMemberData)
    {
        GroupId = groupMemberData.GroupId;
        Rank = groupMemberData.Rank;
        RankName = groupMemberData.RankName;
    }
}
