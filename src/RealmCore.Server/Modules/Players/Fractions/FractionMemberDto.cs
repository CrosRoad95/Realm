namespace RealmCore.Server.Modules.Players.Fractions;

public sealed class FractionMemberDto : IEquatable<FractionMemberDto>
{
    public required int FractionId { get; init; }
    public required int UserId { get; init; }
    public required int Rank { get; init; }
    public required string RankName { get; init; }

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
