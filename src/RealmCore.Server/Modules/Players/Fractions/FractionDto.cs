namespace RealmCore.Server.Modules.Players.Fractions;

public sealed class FractionDto : IEquatable<FractionDto>
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string? Code { get; init; }
    public List<FractionMemberDto> Members { get; init; }

    [return: NotNullIfNotNull(nameof(fractionData))]
    public static FractionDto? Map(FractionData? fractionData)
    {
        if (fractionData == null)
            return null;

        return new FractionDto
        {
            Id = fractionData.Id,
            Name = fractionData.Name,
            Code = fractionData.Code,
            Members = fractionData.Members.Select(FractionMemberDto.Map).ToList()
        };
    }

    public bool Equals(FractionDto? other)
    {
        if (other == null)
            return false;

        return other.Id == Id;
    }
}
