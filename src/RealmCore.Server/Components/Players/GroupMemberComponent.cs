namespace RealmCore.Server.Components.Players;

[ComponentUsage(true)]
public class GroupMemberComponent : Component
{
    public int GroupId { get; private set; }
    public int Rank { get; private set; }
    public string RankName { get; private set; }

    internal GroupMemberComponent(GroupMemberData groupMemberData)
    {
        GroupId = groupMemberData.GroupId;
        Rank = groupMemberData.Rank;
        RankName = groupMemberData.RankName;
    }
}
