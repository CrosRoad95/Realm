namespace RealmCore.Persistence.Data;

public sealed class UserBoostData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BoostId { get; set; }
}

public sealed class UserActiveBoostData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BoostId { get; set; }
    public DateTime ActivatedAt { get; set; }
    public int RemainingTime { get; set; }
}
