namespace RealmCore.Persistence.Data;

public sealed class AchievementData
{
    public int UserId { get; set; }
    public int AchievementId { get; set; }
    public float Progress { get; set; }
    public string? Value { get; set; }
    public bool PrizeReceived { get; set; }
}
