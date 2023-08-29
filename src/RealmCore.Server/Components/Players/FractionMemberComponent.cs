using RealmCore.Persistence.Data;

namespace RealmCore.Server.Components.Players;

[ComponentUsage(true)]
public class FractionMemberComponent : Component
{
    public int FractionId { get; private set; }
    public int Rank { get; private set; }
    public string RankName { get; private set; }

    internal FractionMemberComponent(FractionMemberData fractionMemberData)
    {
        FractionId = fractionMemberData.FractionId;
        Rank = fractionMemberData.Rank;
        RankName = fractionMemberData.RankName;
    }
}
