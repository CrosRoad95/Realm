using GroupMemberData = Realm.Persistance.Data.GroupMember;

namespace Realm.Domain.Components.Players;

public class GroupMemeberComponent : Component
{
    public int GroupId { get; private set; }
    public int Rank { get; private set; }
    public string RankName { get; private set; }

    internal GroupMemeberComponent(GroupMemberData groupMemberData)
    {
        GroupId = groupMemberData.GroupId;
        Rank = groupMemberData.Rank;
        RankName = groupMemberData.RankName;
    }
}
