namespace RealmCore.Server.DTOs;

public class AchievementDTO
{
    public int AchievementId { get; set; }
    public float Progress { get; set; }
    public string? Value { get; set; }
    public bool PrizeReceived { get; set; }
}
