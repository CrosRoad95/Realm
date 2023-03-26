namespace Realm.Persistance.Data;

public sealed class FractionMember
{
    public int FractionId { get; set; }
    public int UserId { get; set; }
    public int Rank { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string RankName { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Fraction? Fraction { get; set; }
    public User? User { get; set; }
}
