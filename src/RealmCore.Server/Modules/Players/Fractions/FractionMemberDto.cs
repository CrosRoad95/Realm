namespace RealmCore.Server.Modules.Players.Fractions;

public sealed class FractionMemberDto : IEquatable<FractionMemberDto>
{
    public int FractionId { get; set; }
    public int UserId { get; set; }
    public int Rank { get; set; }
    public string RankName { get; set; }

    [return: NotNullIfNotNull(nameof(fractionMemberData))]
    public static FractionMemberDto? Map(FractionMemberData? fractionMemberData)
    {
        if (fractionMemberData == null)
            return null;

        return new FractionMemberDto
        {
            FractionId = fractionMemberData.FractionId,
            UserId = fractionMemberData.UserId,
            Rank = fractionMemberData.Rank,
            RankName = fractionMemberData.RankName,
        };
    }

    public bool Equals(FractionMemberDto? other)
    {
        return other?.FractionId == FractionId;
    }
}
