namespace RealmCore.Server.Modules.Players.Fractions;

public sealed class FractionDto : IEquatable<FractionDto>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; }
    public List<FractionMemberDto> Members { get; set; }

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
        return other?.Id == Id;
    }
}
