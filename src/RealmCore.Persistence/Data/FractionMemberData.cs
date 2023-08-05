namespace RealmCore.Persistence.Data;

public sealed class FractionMemberData
{
    public int FractionId { get; set; }
    public int UserId { get; set; }
    public int Rank { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string RankName { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public FractionData? Fraction { get; set; }
    public UserData? User { get; set; }
}
