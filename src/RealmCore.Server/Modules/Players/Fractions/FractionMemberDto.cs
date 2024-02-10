namespace RealmCore.Server.Modules.Players.Fractions;

public sealed class FractionMemberDto : IEquatable<FractionMemberDto>
{
    public int FractionId { get; init; }
    public int UserId { get; init; }
    public int Rank { get; init; }
    public string RankName { get; init; }

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
