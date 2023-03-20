namespace Realm.Persistance.Data;

public sealed class FractionMember
{
    public int FractionId { get; set; }
    public int UserId { get; set; }
    public int Rank { get; set; }
    public string RankName { get; set; }

    public Fraction? Fraction { get; set; }
    public User? User { get; set; }
}
